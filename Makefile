.PHONY: run build engine clean

run:
	dotnet run

build:
	dotnet build

engine:
	cd engine && cmake -B build -DCMAKE_BUILD_TYPE=Release -DCMAKE_INSTALL_PREFIX=/usr --fresh && cmake --build build && sudo cmake --install build

clean:
	rm -rf obj
	rm -rf bin
	rm -rf tests/obj
	rm -rf tests/bin
	rm -rf engine/build
	rm -rf engine/.cache
