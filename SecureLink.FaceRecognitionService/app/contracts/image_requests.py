from pydantic import BaseModel

class GetEmbeddingsRequest(BaseModel):
    file_id: str
    storage_key: str