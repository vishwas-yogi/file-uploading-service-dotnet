from fastapi import FastAPI
from fastapi.concurrency import asynccontextmanager
from .routers import images
from .helpers import config

# load the model before the app starts receiving requests
@asynccontextmanager
async def lifespan(app: FastAPI):
    print("starting app...")
    print("loading the models...")
    config.load_models()
    
    yield

    print("App shutting down...")
    

app = FastAPI(lifespan=lifespan)

app.include_router(images.router)

@app.get("/")
def root():
    return {"message": "App is live"}