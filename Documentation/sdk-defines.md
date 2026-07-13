# SDK Defines

Do not enable a define until the matching SDK exists in the Unity project.

| Define | Needs | Enables |
| --- | --- | --- |
| `FIREBASE` | Firebase Unity SDK | `FirebaseManager` analytics |
| `ADMOB` | Google Mobile Ads Unity SDK | `AdManager` |
| `ADMOB_TEST` | `ADMOB` | Google test ad unit ids |
| `ADMOB_UMP` | `ADMOB`, `UMP` | Omni UMP consent flow |
| `USE_ADMOB_CUSTOM_PLUGIN` | `ADMOB`, JKit/custom native ads | Native/collapse ad adapters |
| `APPFLYER` | AppsFlyer Unity SDK | AppsFlyer event/ad revenue forwarding |
| `IAPPURCHASE_ENABLE` | Unity IAP | `InappPurchase` |

Use `GameFoundation/Configuration/SDK Defines` to toggle these defines.

Legacy projects that still have `ADMOB_UMP` are normalized to `ADMOB_UMP` by the define window.
