serverIsRunning = false
function checkip()
    ip = wifi.sta.getip()
    if ip == nil then
        print("Station Server is requesting ip ...")
    else
        if serverIsRunning == false then
            print("Station Server is running at " .. ip)
            serverIsRunning = true
            tmr.stop(0)
            
        end
    end
end
tmr.alarm(0, 2000, 1, checkip)

nodeIsReady = false
serverip = getServerIp()
function checkin()
    print("Sending checkin request ...")
    chk=net.createConnection(net.TCP, false) 
    chk:on("receive", function(chk, pl)
        print(pl)
        tmr.alarm(2, 1000, 0, closeConn(chk))
    end)
    
    chk:connect(80, serverip)
    
    chipId = node.chipid()
    staMac = wifi.sta.getmac()
    reqString = "/checkin?id=" .. chipId .. "&mac=" .. staMac

    chk:send("GET " .. reqString .. " HTTP/1.1\r\nHost: " .. chipId .. ".local\r\n" .."Connection: keep-alive\r\nPragma: no-cache\r\nCache-Control: no-cache\r\nAccept: */*\r\n\r\n")
end

tmr.alarm(1, 10000, 1, checkin)

function closeConn(chk)
    print("closing con")
    chk:close()
    tmr.stop(2)
end

confServer()