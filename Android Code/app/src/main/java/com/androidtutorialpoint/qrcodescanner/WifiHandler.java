package com.androidtutorialpoint.qrcodescanner;

import android.app.Activity;
import android.content.Context;
import android.net.wifi.WifiConfiguration;
import android.net.wifi.WifiManager;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.widget.Toast;

import java.util.List;

import static android.content.Context.WIFI_SERVICE;

public class WifiHandler {

    private String ssid;
    private String secret;
    private int period;
    private String pshk;
    private TotpCode totpCode;

    private WifiManager wifiManager;

    private OnTotpChanged onTotpChanged;

    private WifiReceiver wifiReceiver;

    private final Context context;

    public WifiHandler(String ssid, String secret, int period, String pshk, final Context context) {
        this.ssid = ssid;
        this.secret = secret;
        this.period = period;
        this.pshk = pshk;
        this.context = context;

        this.wifiManager = (WifiManager) context.getSystemService(WIFI_SERVICE);
        this.totpCode = new TotpCode(this.context);

        wifiReceiver = new WifiReceiver(context);
        wifiReceiver.enable(context);
        wifiReceiver.setWifiDisabled(new WifiReceiver.WifiDisabled() {
            @Override
            public void onWifiDisabled() {
                try{
                    if(getRemainTime() < 30) {
                        connectNextWifi();
                        Toast.makeText(context, "Reconnectando el wifi", Toast.LENGTH_LONG).show();
                    }
                } catch(Exception e) {
                    e.printStackTrace();
                }

            }
        });
    }

    // try to connect wifi with wifi manager
    public void connectWifi() throws Exception {
        String key = this.totpCode.getTOTPKey(this.secret, this.period, this.pshk, System.currentTimeMillis());
        if (onTotpChanged != null){
            onTotpChanged.onTotpChanged(key);
        }
        setWifiConnection(key);
    }

    public void connectNextWifi() throws  Exception {
        int periodMili = (int) this.period;
        String nextKey = this.totpCode.getTOTPKey(this.secret, this.period, this.pshk, System.currentTimeMillis() + periodMili);
        setWifiConnection(nextKey);
    }

    private long getRemainTime() {
        final long desync = (System.currentTimeMillis() /1000) % (period / 1000);
        final long time = (period - (desync * 1000)) / 1000;
        return time;
    }

    private void setWifiConnection(final String key) {
        if (!wifiManager.isWifiEnabled()) {
            wifiManager.setWifiEnabled(true);
        }
        WifiConfiguration conf = new WifiConfiguration();
        conf.SSID =  String.format("\"%s\"", ssid);
        conf.preSharedKey =  String.format("\"%s\"", key);
        int wifiNetwork;
        int myNetworkid = getMyNetworkId();
        if (myNetworkid != -1) {
            conf.networkId = myNetworkid;
            wifiNetwork = wifiManager.updateNetwork(conf);
        } else {
            wifiNetwork = wifiManager.addNetwork(conf);
        }
        wifiManager.disconnect();
        wifiManager.enableNetwork(wifiNetwork, true);
        wifiManager.reconnect();
    }


    private int getMyNetworkId () {
        List<WifiConfiguration> wifiProfilesList;
        wifiProfilesList = wifiManager.getConfiguredNetworks();
        int myNetworkID = -1;

        // TODO: Check if it is possible change it
        for (int i=0; i<wifiProfilesList.size();i++) {
            String profile = wifiProfilesList.get(i).SSID;
            profile = profile.replace("\"", "");
            if (profile.equals(this.ssid)) {
                myNetworkID = wifiProfilesList.get(i).networkId;
                return myNetworkID;
            }
        }
        return myNetworkID;
    }

    public void setOnTotpChanged(OnTotpChanged onTotpChanged) {
        this.onTotpChanged = onTotpChanged;
    }

    public interface OnTotpChanged {
        void onTotpChanged(final String key);
    }

}
