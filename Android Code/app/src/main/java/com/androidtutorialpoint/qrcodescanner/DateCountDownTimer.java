package com.androidtutorialpoint.qrcodescanner;

import android.os.CountDownTimer;
import android.widget.TextView;

public class DateCountDownTimer extends CountDownTimer {

    private TextView countdown;
    private int interval;

    public DateCountDownTimer(long startTime, long interval, TextView countdownView, long period)
    {
        super(startTime, interval);
        this.countdown = countdownView;
        this.interval = (int) period / 1000;
    }

    @Override
    public void onFinish()
    {
        countdown.setText("0");
        this.cancel();
    }

    @Override
    public void onTick(long millisUntilFinished)
    {
        int seconds = (int) (millisUntilFinished / 1000) % interval ;
        countdown.setText(String.valueOf(seconds));
    }
}