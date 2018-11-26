-- Copyright 2008 Steven Barth <steven@midlink.org>
-- Copyright 2011 Jo-Philipp Wich <jow@openwrt.org>
-- Licensed to the public under the Apache License 2.0.

--local sys   = require "luci.sys"
--local conf  = require "luci.config"
local nixio = require "nixio"

local m, globalSection, pairedSection

m = Map("latch", "Latch", translate("Here you can configure Latch."))

local currentSecret = m.uci:get("latch","global", "secret")
local currentAppId = m.uci:get("latch", "global", "appId")

if currentSecret and currentSecret ~= '' and currentAppId and currentAppId ~= ''  then
	pairedSection = m:section(NamedSection, luci.dispatcher.context.authuser, "paired_user", translate("User pairing"))
	pairedSection.anonymous = true
	pairedSection.addremove = false
	
	local currentAccountId = m.uci:get("latch", luci.dispatcher.context.authuser, "account_id")
	if currentAccountId == nil or currentAccountId == '' then
		user_token = pairedSection:option(Value, "token", "Pairing token")
		user_token.datatype = "rangelength(6,6)"
		user_token.rmempty = true
		user_token.write = function(self, section, value)
						   end
							
		pair_button = pairedSection:option(Button, "_pairButton")
		pair_button.title = " "
		pair_button.inputtitle = translate("Pair")
		pair_button.inputstyle = "apply"
		pair_button.write = function(self)
								token = tostring(user_token:formvalue(pairedSection.section))
								if token and string.len(token) == 6 then
									local handle = io.popen("/root/latch.sh pair " .. token)
									local account = luci.util.trim(handle:read("*a"))
									handle:close()
									
									if account and account ~= '' then
										m.uci:set("latch", luci.dispatcher.context.authuser, "account_id", account)
										m.uci.save("latch")
									else
										m.message = translate("Cannot be paired. Try again later")
									end
								
								else
									m.message = translate("Error! Invalid token")
								end
							end
	else
		user_accountid = pairedSection:option(Value, "account_id", "Account id")
		user_accountid.readonly = true
		
		unpair_button = pairedSection:option(Button, "_unpairButton")
		unpair_button.title = " "
		unpair_button.inputtitle = translate("Unpair")
		unpair_button.inputstyle = "remove"
		unpair_button.write = function(self)
								luci.sys.call("/root/latch.sh unpair " .. currentAccountId .. " &")
								m.uci:set("latch", luci.dispatcher.context.authuser, "account_id", "")
								m.uci.save("latch")
							  end
	end
end

globalSection = m:section(NamedSection,"global", "global", translate("Global configuration"))
globalSection.anonymous = true
globalSection.addremove = false

appId = globalSection:option(Value, "appId", "AppId")
appId.datatype = "rangelength(20,20)"
appId.rmempty = true

secret = globalSection:option(Value, "secret", translate("Secret"))
secret.datatype = "rangelength(40,40)"
secret.password = true
secret.rmempty = true

m.on_after_save = function(self)
	if not m.message then
		luci.http.redirect(luci.dispatcher.build_url("admin/system/latch"))
	end
end
return m
