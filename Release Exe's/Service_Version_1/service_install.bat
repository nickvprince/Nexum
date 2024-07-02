sc create nexumservice binpath="c:\program files\nexum\nexum_service.exe" start=auto

sc failure "NexumService" reset= 600 actions= restart/60000/restart/60000/restart/60000