#!/bin/sh

sub_help(){
    echo "Usage: $ProgName <subcommand> [options]\n"
    echo "Subcommands:"
    echo "    pair <token>   Pairs user"
    echo "    unpair <accountId>   Unpairs user"
	echo "    status <accountId>   Check user status"
    echo ""
    echo "For help with each subcommand run:"
    echo "$ProgName <subcommand> -h|--help"
    echo ""
}

send_latch_request(){
	local out=$2
	
	secret=`uci get latch.global.secret`
	appId=`uci get latch.global.appId`

	if [[ ! $secret ]]; then echo 'Missing secret parameter';return 1; fi
	if [[ ! $appId ]]; then echo 'Missing appId parameter';return 1; fi

	server="https://latch.elevenpaths.com"

	requestSignature="GET\n"
	date=`date -u '+%Y-%m-%d %H:%M:%S'`
	requestSignature=$requestSignature$date"\n\n"$1

	signed=`echo -en "$requestSignature" | openssl dgst -sha1 -hmac "$secret" -binary`
	b64signed=`echo -n "$signed"|openssl enc -base64`

	auth_header="Authorization:11PATHS $appId $b64signed"
	date_header="X-11Paths-Date: $date"

	response=`curl -k -q -s -N --header "$auth_header" --header "$date_header" "$server$1"`

	eval $out="'$response'"
}

sub_pair(){
	local jsonPair
    send_latch_request "/api/1.0/pair/$1" jsonPair
	st=`echo $jsonPair|jsonfilter -e '$.data.accountId'`
	echo $st
}
  
sub_unpair(){
	local jsonUnpair
    send_latch_request "/api/1.0/unpair/$1" jsonUnpair
	echo $jsonUnpair
}

sub_status(){
	local jsonStatus
    send_latch_request "/api/1.0/status/$1" jsonStatus
	st=`echo $jsonStatus|jsonfilter -e '$.data.operations.*.status'`
	echo $st
}

ProgName=$(basename $0)
subcommand=$1
case $subcommand in
    "" | "-h" | "--help")
        sub_help
        ;;
    *)
        shift
        sub_${subcommand} $@
        if [ $? = 127 ]; then
            echo "Error: '$subcommand' is not a known subcommand." >&2
            echo "       Run '$ProgName --help' for a list of known subcommands." >&2
            exit 1
        fi
        ;;
esac