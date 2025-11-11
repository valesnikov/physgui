.PHONY: run build engine clean test publish

run:
	dotnet run --project src/PhysGui/

build:
	dotnet build

test:
	dotnet test

publish:
	dotnet publish src/PhysGui/PhysGui.csproj -c Release -r linux-x64 --self-contained true -o ./publish
	cmake -B .cbuild -S src/flphys -DCMAKE_C_FLAGS="-ffast-math" -DCMAKE_BUILD_TYPE=Release --fresh
	cmake --build .cbuild
	cmake --install .cbuild --prefix ./publish --strip

engine:
	cmake -B src/flphys/build -S src/flphys -DCMAKE_C_FLAGS="-ffast-math -march=native" -DCMAKE_BUILD_TYPE=Release --fresh -G Ninja
	cmake --build src/flphys/build
	sudo cmake --install src/flphys/build --prefix /usr/lib --strip

clean:
	rm -rf publish
	rm -rf src/PhysGui/obj
	rm -rf src/PhysGui/bin
	rm -rf tests/PhysGui.Tests/obj
	rm -rf tests/PhysGui.Tests/bin
	rm -rf .cbuild