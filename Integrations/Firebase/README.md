/// <summary>
/// Firebase Manager - Tracking analytics và events
/// 
/// SETUP:
/// 1. Import Firebase SDK (FirebaseAnalytics.unitypackage)
/// 2. Add google-services.json (Android) hoặc GoogleService-Info.plist (iOS) vào Assets/
/// 3. Thêm "FIREBASE" vào Scripting Define Symbols
/// 
/// KHỞI TẠO:
/// await FirebaseManager.I.InitializeAsync();
/// 
/// SỬ DỤNG:
/// // Event đơn giản
/// FirebaseManager.I.TrackingEvent("level_completed");
/// 
/// // Event với parameters
/// FirebaseManager.I.TrackingEvent("level_completed", 
///     ("level_id", 5), 
///     ("score", 1000), 
///     ("stars", 3)
/// );
/// 
/// // Tracking ads
/// FirebaseManager.I.TrackingAdsEvent("rewarded_ad_shown", "main_menu", "completed");
/// 
/// LƯU Ý:
/// - Events tự động queue nếu Firebase chưa init
/// - Chỉ compile khi có FIREBASE define symbol
/// - Xem events trong Firebase Console > Analytics > Events
/// </summary>