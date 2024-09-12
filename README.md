# RAG Commander

A RAG Pattern playground designed to hold customer conversations about the parameters that are involved in splitting text into chunks, recalling the chunks, and using them to answer a user's question.

## Backend

- NET 8

## Frontend

```json
"dependencies": {
    "@solid-primitives/storage": "^2.1.1",
    "axios": "^1.6.2",
    "gpt-tokenizer": "^2.1.2",
    "solid-js": "^1.8.7"
  },
```

## `.env` file or env in docker

```bash
OPENAI_API_URI=https://<NAME>.openai.azure.com/
OPENAI_API_KEY=<KEY>
OPENAI_API_GPT=<GPT_DEPLOYMENT_NAME>
OPENAI_API_ADA=<ADA_DEPLOYMENT_NAME>
DB_MEMORY_PATH=data/
DB_KVC_STORE_PATH=data/
```

## Make file

```bash
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

TAG=0.0.1
docker-build: build-ui
	@echo "Docker"
	cd src/backend && docker build . -t am8850/ragcommander:$(TAG)

docker-run: docker-build
	@echo "Docker run"
	cd src/backend && docker run --rm -p 8080:8080 --env-file=.env-local am8850/ragcommander:$(TAG)

docker-push: docker-build
	docker push am8850/ragcommander:${TAG}

RG_NAME=rg-skragc-poc-eus
APP_NAME=tokenizer
IMAGE_NAME=am8850/ragcommander:$(TAG)
docker-deploy: docker-push	
	az webapp config container set --name $(APP_NAME) --resource-group ${RG_NAME} --docker-custom-image-name ${IMAGE_NAME}
	sleep 2
	az webapp stop --name $(APP_NAME) --resource-group ${RG_NAME}
	sleep 2
	az webapp start --name $(APP_NAME) --resource-group ${RG_NAME}
```

##
