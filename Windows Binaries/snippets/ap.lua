responseHeader = function(code, type, content)
    return "HTTP/1.1 " .. code .. "\r\nConnection: close\r\nServer: eLua-miniweb\r\nContent-Type: " .. type .. "\r\nContent-Length:" .. tostring(string.len(content)) .. "\r\n\r\n"..content; 
end

sendFileContents = function(fileName)
    htmlResult = ""
    if file.open(fileName, "r") then
        repeat 
            local line=file.readline() 
            if line then 
                htmlResult = htmlResult .. line;
            end 
        until not line 
        file.close();
        return htmlResult
    else
        return "Failed to open " .. fileName
    end
end

confServer = function()
    srv=net.createServer(net.TCP)
    srv:listen(80, function(conn)
        conn:on("receive", function(conn, request)  

            reqdata = {};
            _, _, method, req, major, minor = string.find(request, "([A-Z]+) (.+) HTTP/(%d).(%d)"); 
            
            if req:find("%?") then
                local rest
                _, _, fname, rest = req:find("(.*)%?(.*)");

                rest = rest .. "&";
                for crtpair in rest:gmatch("[^&]+") do
                    local _, __, k, v = crtpair:find("(.*)=(.*)");
                    -- replace all "%xx" characters with their actual value
                    v = v:gsub("(%%%x%x)", function(s) return string.char(tonumber(s:sub(2, -1), 16)) end);
                    reqdata[k] = v;
                end
                
                if reqdata.ssid ~= nil and reqdata.pwd ~= nil and reqdata.server_ip then
                    file.open("si.lua", "w")
                    serverInfo = "getServerIp = function() return \"" .. reqdata.server_ip .. "\" end"
                    file.write(serverInfo)
                    file.close()

                    wifi.sta.config(reqdata.ssid, reqdata.pwd)

                    content = "<h1>Success</h1><p>Node Successfully configured. Please restart ESP...</p>"
                    conn:send(responseHeader("200 OK", "text/html", content))
                else
                    content = "<h1>Error</h1><p>Some information is missing. Please restart esp...</p>"
                    conn:send(responseHeader("200 OK", "text/html", content))
                end
                
            else
                content = sendFileContents("setup.lua")
                conn:send(responseHeader("200 OK", "text/html", content))
                content = nil
            end
        end)
        conn:on("sent",function(conn) 
            conn:close()
        end)
    end)
end
confServer()
apIp = wifi.ap.getip()
if apIp ~= nil then
    print("Access Point Server running at " .. apIp)
else
    print("Access Point Server is not running")
end