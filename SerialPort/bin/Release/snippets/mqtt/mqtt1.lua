-- init mqtt client with keepalive timer 120sec
m = mqtt.Client("clientid") -- , 120, "user", "password"

-- setup Last Will and Testament (optional)
-- Broker will publish a message with qos = 0, retain = 0, data = "offline" 
-- to topic "/lwt" if client don't send keepalive packet
m:lwt("/lwt", "offline", 0, 0)

m:on("connect", function(con) print ("connected") end)
m:on("offline", function(con) print ("offline") end)

-- on publish message receive event
m:on("message", function(conn, topic, data) 
  print(topic .. ":" ) 
  if data ~= nil then
    print(data)
  end
end)

m:connect("192.168.1.4", 5000, 0, function(conn) print("connected") end)

-- subscribe topic with qos = 0
m:subscribe("/test",0, function(conn) print("subscribe success") end)

-- publish a message with data = hello, QoS = 0, retain = 0
m:publish("/test","hello",0,0, function(conn) print("sent") end)

--m:close();
-- you can call m:connect again