.PHONY: run build engine clean test publish

run:
	dotnet run

build:
	dotnet build

test:
	dotnet test

publish:
	dotnet publish physgui.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o ./publish

engine:
	cmake -B engine/build -S engine -DCMAKE_C_FLAGS="-ffast-math" -DCMAKE_BUILD_TYPE=Release -DCMAKE_INSTALL_PREFIX=/usr --fresh
	cmake --build engine/build
	sudo cmake --install engine/build

clean:
	rm -rf obj
	rm -rf bin
	rm -rf publish
	rm -rf tests/obj
	rm -rf tests/bin
	rm -rf engine/build
	rm -rf engine/.cache
