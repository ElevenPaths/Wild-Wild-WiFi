package com.androidtutorialpoint.qrcodescanner;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.net.ConnectivityManager;
import android.net.LinkProperties;
import android.net.Network;
import android.net.NetworkCapabilities;
import android.net.NetworkRequest;
import android.net.wifi.WifiManager;
import android.util.Log;

public class WifiReceiver extends ConnectivityManager.NetworkCallback {

    private ConnectivityManager manager;

    private WifiDisabled wifiDisabled;

    final NetworkRequest networkRequest;

    public WifiReceiver(Context context) {
        networkRequest = new NetworkRequest.Builder().addTransportType(NetworkCapabilities.TRANSPORT_WIFI).build();
        manager = (ConnectivityManager) context.getSystemService(context.CONNECTIVITY_SERVICE);
    }

    public void enable(Context context) {
        ConnectivityManager connectivityManager = (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);
        connectivityManager.registerNetworkCallback(networkRequest , this);
    }

    @Override
    public void onLost(Network network) {
        super.onLost(network);
        wifiDisabled.onWifiDisabled();
    }

    void setWifiDisabled(WifiDisabled wifiDisabled) {
        this.wifiDisabled = wifiDisabled;
    }

    interface WifiDisabled {
        void onWifiDisabled();
    }
}
