require "TSLib"
w,h = getScreenSize()
f = readFileString("/private/var/mobile/Media/1ferver/lua/scripts/MemberCode"); -- lấy bundleid con off
repeatTIme = f[2]; -- số giây chờ app chạy
repeat
	runApp(f[1]);
	math.randomseed(os.time());
	tap(math.random (1, w),math.random (1, h));
	mSleep(1000);
until repeatTIme <= 1