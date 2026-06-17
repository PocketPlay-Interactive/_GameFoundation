using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InappProductList", menuName = "IAP/InappProductList")]
public class InappProductList : ScriptableObject
{
    public List<InappProduct> Products;
}