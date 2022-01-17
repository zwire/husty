from __future__ import annotations
from yolo_utils import YoloResult, Drawmode
import cv2, yolo_utils
import numpy as np

class Yolov3v4Darknet:

    def __init__(
        self, 
        cfg: str, 
        weights: str, 
        names: str, 
        blobsize: tuple[int, int], 
        drawmode: Drawmode = Drawmode.rectangle, 
        conf_thresh: float = 0.5, 
        nms_thresh: float = 0.3
    ):
        self.net = cv2.dnn.readNetFromDarknet(cfg, weights)
        self.net.setPreferableBackend(cv2.dnn.DNN_BACKEND_OPENCV)
        self.net.setPreferableTarget(cv2.dnn.DNN_TARGET_CPU)
        self.blobsize = blobsize
        self.conf_thresh = conf_thresh
        self.nms_thresh = nms_thresh
        self.drawmode = drawmode
        self.classes = open(names).read().strip().split('\n')
        np.random.seed(1)
        self.colors = np.random.randint(0, 255, size=(len(self.classes), 3), dtype='uint8')

    def run(self, frame) -> list[YoloResult]:
        blob = cv2.dnn.blobFromImage(frame, 1.0 / 255, self.blobsize, 0, True, False)
        self.net.setInput(blob)
        ln = self.net.getLayerNames()
        ln = [ln[i - 1] for i in self.net.getUnconnectedOutLayers()]
        outs = self.net.forward(ln)
        h, w = frame.shape[:2]
        outs = np.concatenate(outs, 0)
        outs[..., :4] *= np.array([w, h, w, h])
        boxes, confs, probs, ids = yolo_utils.get_filtered_result(outs, self.conf_thresh)
        indices = cv2.dnn.NMSBoxes(boxes, confs, self.conf_thresh, self.nms_thresh)
        results = []
        if len(indices) > 0:
            for i in indices.flatten():
                results.append(YoloResult(boxes[i], confs[i], self.classes[ids[i]], probs[i]))
                yolo_utils.draw(frame, boxes[i], self.classes[ids[i]], self.colors[ids[i]], confs[i], self.drawmode)
        return results