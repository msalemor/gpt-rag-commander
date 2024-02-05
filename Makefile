default:
	@echo "Do something"

clean:
	rm -rf src/backend/wwwroot
	mkdir src/backend/wwwroot

build-ui: clean
	@echo "Build UI"
	cd src/frontend && npm run build-prod
	cp -r src/frontend/dist/* src/backend/wwwroot

dev-backend:
	@echo "Run"
	cd src/backend && dotnet watch run

run-ui:
	@echo "Run UI"	
	cd src/frontend && bun run dev

TAG=0.0.8
docker-build: build-ui
	@echo "Docker"
	cd src/backend && docker build . -t am8850/gptragplayground:$(TAG)

docker-run: docker-build
	@echo "Docker run"
	cd src/backend && docker run --rm -p 8080:8080 --env-file=.env-local am8850/gptragplayground:$(TAG)

docker-push: docker-build
	docker push am8850/gptragplayground:${TAG}

RG_NAME=rg-skragc-poc-eus
APP_NAME=tokenizer
IMAGE_NAME=am8850/gptragplayground:$(TAG)
docker-deploy: docker-push	
	az webapp config container set --name $(APP_NAME) --resource-group ${RG_NAME} --docker-custom-image-name ${IMAGE_NAME}
	sleep 2
	az webapp stop --name $(APP_NAME) --resource-group ${RG_NAME}
	sleep 2
	az webapp start --name $(APP_NAME) --resource-group ${RG_NAME}
