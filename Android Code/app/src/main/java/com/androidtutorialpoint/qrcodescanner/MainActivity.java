package com.androidtutorialpoint.qrcodescanner;

import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.net.wifi.WifiConfiguration;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.os.Handler;
import android.os.Looper;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.CardView;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import org.apache.commons.codec.binary.Base32;
import org.apache.commons.codec.binary.Base64;

import java.net.URI;
import java.nio.ByteBuffer;
import java.security.InvalidKeyException;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Date;
import java.util.List;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;

public class MainActivity extends AppCompatActivity {

    private Button button;
    private Button buttonReconnect;
    private TextView countdown;
    private TextView totpkey;
    private TextView expiresLbl;
    private TextView countdownSeconds;
    private CardView cardView;

    private WifiHandler wifiHandler;

    private final int REQUEST_CODE = 10;
    private final String SHARED_PREF = "SARED_PREF";
    private final String URI_CODE = "URI_CODE";

    SharedPreferences preferences;
    SharedPreferences.Editor preferencesEdit;

    private Handler handler = new Handler();
    Runnable runnable;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        countdown = findViewById(R.id.text_countdown);
        totpkey = findViewById(R.id.text_totp);
        button = findViewById(R.id.button);
        cardView = findViewById(R.id.card_totp);
        expiresLbl = findViewById(R.id.countdown_expires);
        countdownSeconds = findViewById(R.id.countdown_seconds);
        buttonReconnect = findViewById(R.id.button_reconnect);

        preferences= getSharedPreferences(SHARED_PREF, 0);
        preferencesEdit = preferences.edit();

        button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent i = new Intent(MainActivity.this, QrCodeScannerActivity.class);
                startActivityForResult (i, REQUEST_CODE);
            }
        });

        buttonReconnect.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (wifiHandler != null) {
                    try{
                        wifiHandler.connectWifi();
                    }catch (Exception e) {
                        Log.e("Except", "Exception");
                    }

                }
            }
        });

        buttonReconnect.setOnLongClickListener(new View.OnLongClickListener() {
            @Override
            public boolean onLongClick(View v) {
                if (wifiHandler != null) {
                    try{
                        wifiHandler.connectNextWifi();
                    }catch (Exception e) {
                        Log.e("Except", "Exception");
                    }

                }
                return true;
            }

        });

        // TODO: Check if there's a uri added
        checkUriSaved();
    }

    private void checkUriSaved() {
        try {
            Uri savedUri = Uri.parse(preferences.getString(URI_CODE, "defaultString"));
            refreshActivity(true);
            runWildWildWifi(savedUri);

        } catch (IllegalArgumentException e){
            Log.e("Except", "No se ha añaido una url todavia:" + e);
            refreshActivity(false);
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == REQUEST_CODE) {
            // Make sure the request was successful
            if (resultCode == RESULT_OK) {
                this.refreshActivity(true);
                // get uri of totp
                final String rawResult = data.getExtras().getString("uri");
                // get uri
                Uri myURI = Uri.parse(rawResult);
                saveUri(myURI);
                refreshActivity(true);
                if (runnable != null) {
                    handler.removeCallbacks(runnable);
                }
                runWildWildWifi(myURI);
            }
        }
    }


    private void runWildWildWifi(Uri wildUri) {
        final QRInfo qrInfo = new QRInfo(wildUri);
        wifiHandler = new WifiHandler(qrInfo.ssid, qrInfo.secret, qrInfo.period, qrInfo.pshk, getApplicationContext());

        wifiHandler.setOnTotpChanged(new WifiHandler.OnTotpChanged() {
            @Override
            public void onTotpChanged(final String key) {
                new Handler(Looper.getMainLooper()).post(new Runnable() {
                    @Override
                    public void run() {
                        totpkey.setText(key);
                    }
                });

            }
        });

        // background thread
        runnable = new Runnable() {
            @Override
            public void run() {
                try {
                    wifiHandler.connectWifi();
                    long sleepmseconds = getRemainTime(qrInfo.period);
                    DateCountDownTimer timer = new DateCountDownTimer(sleepmseconds, 1000, countdown, qrInfo.period);
                    timer.start();
                    handler.postDelayed(this, sleepmseconds);
                } catch (Exception e) {
                    Log.e("Except", "Exception:" + e);
                    e.printStackTrace();
                }
            }
        };
        handler.post(runnable);
    }

    private long getRemainTime(long period) {
        final long desync = (System.currentTimeMillis() /1000) % (period / 1000);
        return period - (desync * 1000);
    }

    private void saveUri(Uri totpUri) {
        preferencesEdit.putString(URI_CODE, totpUri.toString());
        preferencesEdit.commit();
    }

    // Control the UI to show or hide elements
    private void refreshActivity(Boolean isTotpAdded) {
        int visibility = isTotpAdded ? View.VISIBLE : View.INVISIBLE;
        countdown.setVisibility(visibility);
        totpkey.setVisibility(visibility);
        cardView.setVisibility(visibility);
        expiresLbl.setVisibility(visibility);
        countdownSeconds.setVisibility(visibility);
    }

}

