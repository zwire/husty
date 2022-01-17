from __future__ import annotations
import enum, cv2
import numpy as np

class YoloResult:
    
    def __init__(self, box: list[int], confidence: float, label: str, probability: float):
        self.box = box
        self.confidence = confidence
        self.label = label
        self.probability = probability

    def __str__(self) -> str:
        return f'label: {self.label}, box: {self.box}, conf: {self.confidence * 100:.1f}%, prob: {self.probability * 100:.1f}%'

class Drawmode(enum.Enum):

    off = 0
    point = 1
    rectangle = 2

def get_filtered_result(outputs, conf_thresh: float) -> tuple[list[int], float, float, int]:
    boxes, confs, probs, ids = [], [], [], []
    for detection in filter(lambda x: x[4] > conf_thresh, outputs):
        scores = detection[5:]
        id = np.argmax(scores)
        prob = np.max(scores)
        conf = detection[4]
        box = detection[:4]
        centerX, centerY, width, height = box.astype("int")
        box = [int(centerX - (width / 2)), int(centerY - (height / 2)), int(width), int(height)]
        boxes.append(box)
        confs.append(float(conf))
        probs.append(prob)
        ids.append(id)
    return boxes, confs, probs, ids

def draw(frame, box: list[int], label: str, color: tuple[int, int, int], conf: float, mode: Drawmode):
    x, y = box[0], box[1]
    w, h = box[2], box[3]
    color = [int(c) for c in color]
    if mode == Drawmode.rectangle:
        cv2.rectangle(frame, (x, y), (x + w, y + h), color, 2)
        cv2.putText(frame, f"{label}:{conf * 100:.1f}%", (x, y - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.3, (0, 0, 0), 1)
    elif mode == Drawmode.point:
        cv2.circle(frame, (int(x + w / 2), int(y + h / 2)), 3, color, 4)