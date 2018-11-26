#!/bin/sh

secretKey=`uci get wireless.@wifi-iface[0].totp_secret`
stepSeconds=`uci get wireless.@wifi-iface[0].totp_step_seconds`
pshk=`uci get wireless.@wifi-iface[0].totp_pshk`

if [[ ! $secretKey ]]; then echo 'Missing secretKey parameter';return 1; fi
if [[ ! $stepSeconds ]]; then echo 'Missing stepSeconds parameter';return 1; fi
if [[ ! $pshk ]]; then echo 'Missing pshk parameter';return 1; fi

currentSecondsOfDay=`date -u +%T | awk -F ":" '{print ($1 * 3600)+ ($2 * 60) + $3}'`
nextSecondsChange=$currentSecondsOfDay


function generateNewKey()
{
	local pshk=$1
	local secret=$2
	local stepSeconds=$3
    local  __resultvar=$4
	
	local otp=`oathtool -b $secret --totp -s $stepSeconds -d 8`
	local otphash=`echo -n $otp | sha1sum | sed 's/ .*//'`
	local pshkhash=`echo -n $pshk | sha1sum | sed 's/ .*//'`
	local finalkey=`echo -n $otphash$pshkhash | sha1sum | sed 's/ .*//'`
	
	echo 'New otp: ' $otp ' - ' $finalkey ' - Time: ' `date -u +%H:%M:%S`
    eval $__resultvar="'$finalkey'"
}

while true; do
  secToSleep=0.2
  if [ $currentSecondsOfDay -ge $nextSecondsChange ]; then
	generateNewKey $pshk $secretKey $stepSeconds newKey
    uci set wireless.@wifi-iface[0].key=$newKey
    uci commit wireless
    wifi
    desync=$(($currentSecondsOfDay % $stepSeconds))
    nextSecondsChange=$(($currentSecondsOfDay + $stepSeconds - $desync))
    echo 'Next change: ' `date -u -d@$nextSecondsChange -u +%H:%M:%S`
    secToSleep=$(($stepSeconds - $desync - 1))
	echo 'Sleeping seconds: ' $secToSleep
  fi
  
  /usr/bin/sleep $secToSleep
  currentSecondsOfDay=`date -u +%T | awk -F ":" '{print ($1 * 3600)+ ($2 * 60) + $3}'`
done
