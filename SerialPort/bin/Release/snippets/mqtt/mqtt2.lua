topics = {"test1","test2","test3","test4","test5","test6"} --array of topics
current_topic = 1 -- variable for one currently being subscribed to
alarm_delay = 50 -- microseconds between subscription attempts, worked for me (local network) down to 5...YMMV

--connect to the broker
m:connect("192.168.11.10", 1883, 0, function(conn)
     print("connected")
     mqtt_sub() --run the subscription function
end)

function mqtt_sub()

     -- if we have subscribed to all topics in the array, run the main prog
     if table.getn(topics) < current_topic then
     
          run_main_prog()
         
     else
          --subscribe to the topic
          m:subscribe(topics[current_topic],0, function(conn)
               print("subscribe")
          end)
          -- increment the variable of the current topic for next loop     
          current_topic = current_topic + 1
          --set the timer to rerun the loop
          tmr.alarm(0, alarm_delay, 0, function()
               mqtt_sub()
          end)
     end
end

--main program to run after the subscriptions are done
function run_main_prog()
     print("Hello World")
end