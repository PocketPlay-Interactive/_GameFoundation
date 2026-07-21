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
    private bool isInitializing;
    private bool isInitialized;
    private bool storeEventsRegistered;
#endif

    private bool logDebug = true;
    private int lastPurchaseAttemptTimestamp = 0;

    public async void InitializePurchasing()
    {
#if IAPPURCHASE_ENABLE
        if (isInitialized || isInitializing)
        {
            Log("[InappPurchase] InitializePurchasing skipped. Already initialized or initializing.");
            return;
        }

        isInitializing = true;
        try
        {
            ProductList = ProductList ?? ResourceHandle.Get().LoadData<InappProductList>("InappProductList");
            storeController = storeController ?? UnityIAPServices.StoreController();
            RegisterStoreEvents();
            await storeController.Connect();
            isInitializing = false;
            isInitialized = true;
            var productDefs = Products.Select(p => new ProductDefinition(p.ProductId, p.Type)).ToList();
            storeController.FetchProducts(productDefs);
        }
        catch (Exception exception)
        {
            isInitializing = false;
            isInitialized = false;
            Log("[InappPurchase] Initialization failed: " + exception.Message);
        }
#endif
        Log("[InappPurchase] InitializePurchasing called");
    }

#if IAPPURCHASE_ENABLE
    private void RegisterStoreEvents()
    {
        if (storeEventsRegistered) return;

        storeController.OnProductsFetched -= OnProductsFetched;
        storeController.OnPurchasesFetched -= OnPurchasesFetched;
        storeController.OnPurchasePending -= OnPurchasePending;
        storeController.OnPurchaseFailed -= OnPurchaseFailed;
        storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;

        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeEventsRegistered = true;
    }

    private void OnProductsFetched(List<Product> products)
    {
        var storeProducts = products?
            .Where(product => product?.definition != null)
            .ToList() ?? new List<Product>();

        isInitializing = false;
        isInitialized = true;
        Log($"[InappPurchase] Store products ready: {storeProducts.Count}/{Products.Count}");
        foreach (var product in storeProducts)
            Log($"[InappPurchase] Store product: {product.definition.id} | {product.definition.type} | {product.metadata?.localizedPriceString}");

        storeController.FetchPurchases();
    }
    
    // Hàm này được gọi khi danh sách đơn hàng đã được fetch từ store.
    // Dùng để xử lý các đơn hàng đã xác nhận, ví dụ: cấp quyền cho người dùng, lưu trạng thái mua.
    private void OnPurchasesFetched(Orders orders)
    {
        if (orders?.ConfirmedOrders == null) return;

        Log("[InappPurchase] Purchases fetched: " + orders.ConfirmedOrders.Count);
        foreach (var order in orders.ConfirmedOrders)
        {
            foreach (var item in order.CartOrdered.Items())
            {
                var product = item?.Product;
                if (!IsRestorableNonConsumable(product)) continue;

                var productId = product.definition.id;
                Debug.Log($"[InappPurchase] Restorable purchase found: {productId}");
                if (RuntimeStorageData.PLAYER.HasProduct(productId)) continue;

                RuntimeStorageData.PLAYER.AddProduct(productId);
                EventBus.Publish(new PurchaseCompletedEvent { ProductId = productId });
            }
        }

        RuntimeStorageData.PLAYER.LogAllProducts();
    }
    
    private void OnPurchasePending(PendingOrder order)
    {
        Log("[InappPurchase] Purchase pending: " + order.CartOrdered);
        var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
        if (product == null) return;

        Debug.Log($"[InappPurchase] Pending purchase: {product.definition.id}");
        storeController.ConfirmPurchase(order);
    }

    private void OnPurchaseConfirmed(Order order)
    {
        var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
        if (product == null) return;

        Debug.Log($"[InappPurchase] Confirmed purchase: {product.definition.id}");
        if (product.definition.id == currentProductId)
            CompleteCurrentPurchase(true);
    }
    
    private void OnPurchaseFailed(FailedOrder order)
    {
        Log("[InappPurchase] Order info: " + order.Details);
        CompleteCurrentPurchase(false);
    }

    private bool IsRestorableNonConsumable(Product product)
    {
        if (product?.definition == null || IsBattlePassProduct(product.definition.id)) return false;

        var configuredProduct = Products.FirstOrDefault(item => item.ProductId == product.definition.id);
        if (configuredProduct == null || configuredProduct.Type != ProductType.NonConsumable) return false;

        return product.definition.type == ProductType.NonConsumable;
    }

    private void CompleteCurrentPurchase(bool complete)
    {
        var callback = OnPurchaseComplete;
        OnPurchaseComplete = null;
        currentProductId = null;
        callback?.Invoke(complete);
    }
#endif

    public bool HasProductId(string productId) => Products.Any(p => p.ProductId == productId);

    public string GetProductIdByIndex(int index) => (index >= 0 && index < Products.Count) ? Products[index].ProductId : "";

    public string GetProductIdByID(string id)
    {
        var product = Products.Find(p => p.ID.ToString() == id);
        if (product == null)
            return null;
        return product.ProductId;
    }

    public string GetLocalizedPrice(string productId)
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

    private bool IsBattlePassProduct(string productId) =>
        Products.FirstOrDefault(item => item.ProductId == productId)?.BattlePass != BattlePassType.None;

    private bool throttleClick()
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
        if (string.IsNullOrEmpty(productId))
        {
            onComplete?.Invoke(false);
            return;
        }

#if IAPPURCHASE_ENABLE
        if (throttleClick())
        {
            onComplete?.Invoke(false);
            return;
        }

        if (OnPurchaseComplete != null)
        {
            onComplete?.Invoke(false);
            return;
        }

        Log($"[InappPurchase] Buy IAP: {productId}");
        if (storeController == null || !isInitialized)
        {
            Log("[InappPurchase] Purchase failed. Store not initialized.");
            onComplete?.Invoke(false);
            return;
        }

        var product = storeController.GetProductById(productId);
        if (product == null)
        {
            Log($"[InappPurchase] Product not found: {productId}");
            onComplete?.Invoke(false);
            return;
        }

        currentProductId = productId;
        OnPurchaseComplete = complete =>
        {
            onComplete?.Invoke(complete);
            if (!complete)
            {
                Log($"[InappPurchase] Purchase failed for product ID: {productId}");
                return;
            }

            if (IsBattlePassProduct(productId)) return;

            RuntimeStorageData.PLAYER.AddProduct(productId);
            EventBus.Publish(new PurchaseCompletedEvent { ProductId = productId });
        };

        try
        {
            storeController.PurchaseProduct(product);
            Log($"[InappPurchase] Purchasing: {productId}");
        }
        catch (Exception exception)
        {
            Log("[InappPurchase] Purchase failed: " + exception.Message);
            CompleteCurrentPurchase(false);
        }
#else
        if (!IsBattlePassProduct(productId))
        {
            RuntimeStorageData.PLAYER.AddProduct(productId);
            EventBus.Publish(new PurchaseCompletedEvent { ProductId = productId });
        }
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
    public BattlePassType BattlePass = BattlePassType.None;
#if IAPPURCHASE_ENABLE
    public UnityEngine.Purchasing.ProductType Type;
#endif
}

public enum BattlePassType
{
    None,
    Weekly,
    Monthly
}
