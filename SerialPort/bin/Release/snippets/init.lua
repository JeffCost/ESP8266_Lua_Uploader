dofile("si.lua")
gpio.mode(3, gpio.OUTPUT)
gpio.mode(4, gpio.OUTPUT)
ip = getServerIp()
mode = wifi.getmode()
if ip ~= nil then 
	if(wifi.STATION ~= mode) then
		print("Set to station restarting...")
		wifi.setmode(wifi.STATION)
		wifi.sta.connect()
		node.restart()
	end
	wifi.sta.connect()
	dofile("sta.lua")
else -- I am station but I dont have server ip
	dofile("ap.lua")
end