responseHeader = function(code, type, content)
    return "HTTP/1.1 " .. code .. "\r\nConnection: close\r\nServer: eLua-miniweb\r\nContent-Type: " .. type .. "\r\nContent-Length:" .. tostring(string.len(content)) .. "\r\n\r\n"..content; 
end
confServer = function()
srv=net.createServer(net.TCP)
srv:listen(80,function(conn)
    conn:on("receive",function(conn, request)  
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

            content = "{"
            if reqdata.get ~= nil then
                pinValue = gpio.read(reqdata.get)
                content = content .. "\"value\": \"" .. pinValue .. "\""
            end
            
            if reqdata.set ~= nil and reqdata.val ~= nil then
                gpio.write(reqdata.set, reqdata.val);
                content = content .. "\"value\": \"" .. reqdata.val .. "\""
            end

            if reqdata.setmode ~= nil and reqdata.val ~= nil then
                val = 0;
                if(reqdata.val > 0) then
                    val = 1
                end
                gpio.mode(reqdata.setmode, val);
                content = content .. "\"value\": \"" .. reqdata.val .. "\""
            end

            content = content .. "}"
            conn:send(responseHeader("200 OK", "application/json", content))
            content = nil
        else
            pinValue3 = gpio.read(3)
            pinValue4 = gpio.read(4)
            content = "{"
            content = content .."\"status\":\"OK\","
            content = content .."\"gpio\": {"
            content = content .."\"gpio0\":\"".. pinValue3 .."\","
            content = content .."\"gpio2\":\"".. pinValue4 .."\""
            content = content .."}"
            content = content .. "}"
            conn:send(responseHeader("200 OK", "application/json", content))
            content = nil
        end

        _, method, req, major, minor, request, reqdata, hb = nil, nil, nil, nil, nil, nil, nil, nil;
        collectgarbage()
    end)

    conn:on("sent",function(conn) 
        conn:close() 
    end)
end)
end

dofile("initsta.lua")
