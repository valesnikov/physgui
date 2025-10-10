.PHONY: run build engine clean test publish

run:
	dotnet run --project src/PhysGui/

build:
	dotnet build

test:
	dotnet test

publish:
	dotnet publish src/PhysGui/PhysGui.csproj -c Release -r linux-x64 --self-contained true -o ./publish
	cc -shared -fPIC -O3 -ffast-math -s -flto -o ./publish/libflphys.so engine/flphys.c

engine:
	cmake -B engine/build -S engine -DCMAKE_C_FLAGS="-ffast-math" -DCMAKE_BUILD_TYPE=Release -DCMAKE_INSTALL_PREFIX=/usr --fresh
	cmake --build engine/build
	sudo cmake --install engine/build

clean:
	rm -rf publish
	rm -rf src/PhysGui/obj
	rm -rf src/PhysGui/bin
	rm -rf tests/PhysGui.Tests/obj
	rm -rf tests/PhysGui.Tests/bin
	rm -rf engine/build
	rm -rf engine/.cache
