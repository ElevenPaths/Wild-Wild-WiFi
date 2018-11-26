package com.androidtutorialpoint.qrcodescanner;

import android.net.Uri;
import android.util.Log;

public class QRInfo {

    public String ssid;
    public String secret;
    public String pshk;
    public int period;

    public QRInfo(Uri uriInfo) {
        this.ssid = uriInfo.getQueryParameter("ssid");
        this.secret = uriInfo.getQueryParameter("secret");
        this.pshk= uriInfo.getQueryParameter("pshk");
        String periodString = uriInfo.getQueryParameter("period");
        this.period = Integer.parseInt(periodString)*1000;
    }

}
