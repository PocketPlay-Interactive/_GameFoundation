using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if IAPPURCHASE_ENABLE
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Services;
#endif


public class InappPurchase : GenericSingleton<InappPurchase>
{
    public InappProductList ProductList; // Kéo ScriptableObject vào đây qua Inspector hoặc Resources.Load

    public List<InappProduct> Products => ProductList != null ? ProductList.Products : new List<InappProduct>();

#if IAPPURCHASE_ENABLE
    private StoreController storeController;
    private string currentProductId;
    private Action<bool> OnPurchaseComplete;
#endif

    private bool logDebug = true;
    private int lastPurchaseAttemptTimestamp = 0;

    public async void InitializePurchasing()
    {
#if IAPPURCHASE_ENABLE
        ProductList = ResourceHandle.I.LoadData<InappProductList>("InappProductList");
        storeController = UnityIAPServices.StoreController();

        // Đăng ký event handler trước khi connect
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        await storeController.Connect();

        var productDefs = Products.Select(p => new ProductDefinition(p.ProductId, p.Type)).ToList();
        storeController.FetchProducts(productDefs);
#endif
        Log("[InappPurchase] InitializePurchasing called");
    }

#if IAPPURCHASE_ENABLE
    private void OnProductsFetched(List<Product> products)
    {
        Log("[InappPurchase] Products fetched: " + products.Count);
        RuntimeStorageData.Player.RemoveAllProducts();
        storeController.FetchPurchases();
    }
    
    // Hàm này được gọi khi danh sách đơn hàng đã được fetch từ store.
    // Dùng để xử lý các đơn hàng đã xác nhận, ví dụ: cấp quyền cho người dùng, lưu trạng thái mua.
    private void OnPurchasesFetched(Orders orders)
    {
        Log("[InappPurchase] Purchases fetched: " + orders.ConfirmedOrders.Count);
        foreach (var order in orders.ConfirmedOrders)
        {
            var product = order.CartOrdered.Items().First()?.Product;
            Debug.Log($"[InappPurchase] Confirmed purchase found: {product.definition.id}");
            RuntimeStorageData.Player.AddProduct(product.definition.id);
            GameEvent.OnIAPurchaseMethod(product.definition.id);
        }

        RuntimeStorageData.Player.LogAllProducts();
    }
    
    private void OnPurchasePending(PendingOrder order)
    {
        Log("[InappPurchase] Purchase pending: " + order.CartOrdered);
        var product = order.CartOrdered.Items().First()?.Product;
        Debug.Log($"[InappPurchase] Pending purchase: {product.definition.id}");
        storeController.ConfirmPurchase(order);
    }

    private void OnPurchaseConfirmed(Order order)
    {
        var product = order.CartOrdered.Items().First()?.Product;
        Debug.Log($"[InappPurchase] Confirmed purchase: {product.definition.id}");

        OnPurchaseComplete?.Invoke(true);
    }
    
    private void OnPurchaseFailed(FailedOrder order)
    {
        Log("[InappPurchase] Order info: " + order.Details);
        OnPurchaseComplete?.Invoke(false);
    }
#endif

    public bool HasProduct(string productId) => Products.Any(p => p.ProductId == productId);

    public string GetProductIdByIndex(int index) => (index >= 0 && index < Products.Count) ? Products[index].ProductId : "";

    public int GetProductIndexById(string id) => Products.FindIndex(p => p.ProductId == id);

    public string GetProductInfo(string productId)
    {
#if IAPPURCHASE_ENABLE
        var product = storeController?.GetProducts()?.FirstOrDefault(p => p.definition.id == productId);
        return product?.metadata.localizedPriceString ?? GetLocalPrice(productId);
#else
        return GetLocalPrice(productId);
#endif
    }

    public string GetProductName(string productId) =>
        Products.FirstOrDefault(p => p.ProductId == productId)?.ProductName ?? "";

    private string GetLocalPrice(string productId) =>
        Products.FirstOrDefault(p => p.ProductId == productId)?.ProductPrice ?? "";

    private bool AntiSpamClick()
    {
        int now = UtilsTimerHelper.CurrentTimeInSeconds();
        if (now - lastPurchaseAttemptTimestamp > 1)
        {
            lastPurchaseAttemptTimestamp = now;
            return false;
        }
        return true;
    }

    public void BuyProduct(string productId, Action<bool> onComplete)
    {
#if IAPPURCHASE_ENABLE
        if (string.IsNullOrEmpty(productId) || AntiSpamClick()) return;
        Log($"[InappPurchase] Buy IAP: {productId}");
        currentProductId = productId;
        OnPurchaseComplete = complete =>
        {
            onComplete?.Invoke(complete);
            if (!complete)
            {
                Log($"[InappPurchase] Purchase failed for product ID: {productId}");
                return;
            }
            RuntimeStorageData.Player.AddProduct(productId);
            GameEvent.OnIAPurchaseMethod(productId);
        };

// #if UNITY_EDITOR
//         // Simulate purchase success in editor
//         Debug.Log($"[InappPurchase] Simulating purchase for product ID: {productId}");
//         OnPurchaseComplete?.Invoke(true);
//         return;
// #endif
        
        if (storeController != null)
        {
            var product = storeController.GetProductById(productId);
            if (product != null)
            {
                storeController.PurchaseProduct(product);
                Log($"[InappPurchase] Purchasing: {productId}");
            }
            else
            {
                Log($"[InappPurchase] Product not found: {productId}");
            }
        }
        else
        {
            Log("[InappPurchase] Not initialized, initializing now...");
            InitializePurchasing();
        }
#else
        onComplete?.Invoke(true);
#endif
    }

    public void RestorePurchases()
    {
#if IAPPURCHASE_ENABLE
        if (storeController == null)
        {
            Log("[InappPurchase] RestorePurchases FAIL. Not initialized.");
            return;
        }
        storeController.FetchPurchases();
#endif
    }

    private void Log(string msg)
    {
        if (logDebug) Debug.Log(msg);
    }
}

[Serializable]
public class InappProduct
{
    public int ID;
    public string ProductId;
    public string ProductName;
    public string ProductPrice;
#if IAPPURCHASE_ENABLE
    public UnityEngine.Purchasing.ProductType Type;
#endif
}