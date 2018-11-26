package com.androidtutorialpoint.qrcodescanner;

import android.content.Context;
import android.util.Log;
import android.widget.Toast;

import org.apache.commons.codec.binary.Base32;

import java.nio.ByteBuffer;
import java.security.InvalidKeyException;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;

public class TotpCode {

    private Context context;

    public TotpCode(Context appContext) {
        this.context = appContext;
    }


    // new totpkey
    public String getTOTPKey(String secreto, int period, String pshk, long time) throws Exception {
        String clave = getCode(secreto, period, time);
        String finalKey = sha1Cipher(clave, pshk);
        return finalKey;
    }

    public String sha1Cipher(String clavetotp, String pshktemp ) {
        try {
            String totpHashed = sha1(clavetotp);
            String pshkHashed = sha1(pshktemp);
            return sha1(totpHashed + pshkHashed);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return "";
    }


    public String getCode(final String sharedSecret, final int period, final long date) {

        final int CODE_LENGHT = 8;
        final String MAC_ALGORITHM = "HmacSHA1";
        try {
            Base32 base32 = new Base32();
            byte[] secret = base32.decode(sharedSecret.toUpperCase());
            if (secret.length > 0) {
                SecretKeySpec signKey = new SecretKeySpec(secret, MAC_ALGORITHM);
                ByteBuffer buffer = ByteBuffer.allocate(8);
                buffer.putLong(date / period);
                byte[] timeBytes = buffer.array();
                Mac mac = Mac.getInstance(MAC_ALGORITHM);
                mac.init(signKey);
                byte[] hash = mac.doFinal(timeBytes);
                int offset = hash[19] & 0xf;
                long truncatedHash = hash[offset] & 0x7f;
                for (int i = 1; i < 4; i++) {
                    truncatedHash <<= 8;
                    truncatedHash |= hash[offset + i] & 0xff;
                }
                String code = String.valueOf((truncatedHash %= 100000000));
                if (code.length() < CODE_LENGHT) {
                    int diff = CODE_LENGHT - code.length();
                    for (int i = 0; i < diff; i++) {
                        code = "0".concat(code);
                    }
                }
                Log.i("getCode","code: " + code);
                return code;
            }
        } catch (NoSuchAlgorithmException e) {
        } catch (InvalidKeyException e) {

        }
        return "------";
    }

    public String sha1(String input) throws Exception {
        MessageDigest mDigest = MessageDigest.getInstance("SHA1");
        byte[] result = mDigest.digest(input.getBytes());
        StringBuffer sb = new StringBuffer();
        for (int i = 0; i < result.length; i++) {
            sb.append(Integer.toString((result[i] & 0xff) + 0x100, 16).substring(1));
        }
        return sb.toString();
    }

}
