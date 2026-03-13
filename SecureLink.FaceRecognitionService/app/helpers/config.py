import numpy as np
from deepface import DeepFace

MODEL_NAME = "Facenet512"
DETECTOR_BACKEND = "retinaface"
ENFORCE_DETECTION = False


def load_models():
    DeepFace.build_model(model_name=MODEL_NAME)

    # Warm up detector
    dummy = np.zeros((224, 224, 3), dtype=np.uint8)

    DeepFace.extract_faces(
        img_path=dummy, detector_backend=DETECTOR_BACKEND, enforce_detection=False
    )


def generate_embeddings(img):
    return DeepFace.represent(
        img_path=img,
        model_name=MODEL_NAME,
        detector_backend=DETECTOR_BACKEND,
        enforce_detection=ENFORCE_DETECTION,
    )
