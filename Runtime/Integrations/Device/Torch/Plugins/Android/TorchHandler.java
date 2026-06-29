package com.unity3d.player;

import android.hardware.camera2.CameraManager;
import android.os.Handler;
import android.os.HandlerThread;
import java.util.concurrent.atomic.AtomicBoolean;

public class TorchHandler {
    private String cameraId;
    private HandlerThread handlerThread;
    private Handler handler;
    private AtomicBoolean isRunning = new AtomicBoolean(false);
    
    // ✅ Chỉ nhận cameraId, không cache CameraManager
    public TorchHandler(String id) {
        this.cameraId = id;
        
        handlerThread = new HandlerThread("TorchThread");
        handlerThread.start();
        handler = new Handler(handlerThread.getLooper());
    }
    
    public void postBlink(final int onMs, final int offMs, final int count) {
        cancelBlink();
        isRunning.set(true);
        
        handler.post(new Runnable() {
            @Override
            public void run() {
                // ✅ Lấy CameraManager mới mỗi lần sử dụng
                CameraManager cameraManager = null;
                try {
                    cameraManager = (CameraManager) UnityPlayer.currentActivity
                        .getSystemService("camera");
                    
                    for (int i = 0; i < count && isRunning.get(); i++) {
                        cameraManager.setTorchMode(cameraId, true);
                        Thread.sleep(onMs);
                        cameraManager.setTorchMode(cameraId, false);
                        
                        if (i < count - 1 && isRunning.get()) {
                            Thread.sleep(offMs);
                        }
                    }
                } catch (InterruptedException e) {
                    if (cameraManager != null) {
                        try {
                            cameraManager.setTorchMode(cameraId, false);
                        } catch (Exception ex) {}
                    }
                } catch (Exception e) {
                    // silent
                } finally {
                    isRunning.set(false);
                    // ✅ CameraManager tự động cleanup bởi GC
                }
            }
        });
    }
    
    public void cancelBlink() {
        isRunning.set(false);
        handler.removeCallbacksAndMessages(null);
        
        // ✅ Lấy CameraManager mới để tắt đèn
        try {
            CameraManager cm = (CameraManager) UnityPlayer.currentActivity
                .getSystemService("camera");
            cm.setTorchMode(cameraId, false);
        } catch (Exception e) {}
    }
    
    public void dispose() {
        cancelBlink();
        if (handlerThread != null) {
            handlerThread.quitSafely();
            try {
                handlerThread.join(100);
            } catch (InterruptedException e) {}
        }
    }
}