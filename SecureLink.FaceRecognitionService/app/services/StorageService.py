from pathlib import Path
from fastapi.concurrency import run_in_threadpool


class StorageService:
    def __init__(self):
        self._base_dir = Path("/home/vishwas-yogi/personal/uploads")
        self._base_dir.mkdir(parents=False, exist_ok=True)

    def _get_full_path(self, storage_key: str) -> Path:
        return self._base_dir / storage_key

    async def download(self, storage_key: str) -> bytes:
        file_path = self._get_full_path(storage_key)
        if not file_path.exists():
            raise FileNotFoundError(f"File {storage_key} not found.")

        def read_file_sync():
            with open(file_path, "rb") as f:
                return f.read()

        return await run_in_threadpool(read_file_sync)
