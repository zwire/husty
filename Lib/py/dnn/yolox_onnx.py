from __future__ import annotations
from yolo_utils import YoloResult, Drawmode
import cv2, onnxruntime, yolo_utils
import numpy as np

class YoloxOnnx:

    def __init__(
        self,
        onnx: str,
        names: str,
        input_shape: tuple[int, int] = (416, 416),
        drawmode: Drawmode = Drawmode.rectangle, 
        conf_thresh: float = 0.5, 
        nms_thresh: float = 0.3,
        with_p6: bool = False,
    ):
        self.input_shape = input_shape
        self.drawmode = drawmode
        self.classes = open(names).read().strip().split('\n')
        np.random.seed(1)
        self.colors = np.random.randint(0, 255, size=(len(self.classes), 3), dtype='uint8')
        self.conf_thresh = conf_thresh
        self.nms_thresh = nms_thresh
        self.with_p6 = with_p6
        self.onnx_session = onnxruntime.InferenceSession(onnx)
        self.input_name = self.onnx_session.get_inputs()[0].name
        self.output_name = self.onnx_session.get_outputs()[0].name

    def run(self, frame: np.ndarray) -> list[YoloResult]:
        h, w = frame.shape[:2]
        padded_frame = np.ones((self.input_shape[0], self.input_shape[1], 3), dtype=np.uint8) * 114
        ratio = min(self.input_shape[0] / h, self.input_shape[1] / w)
        resized_frame = cv2.resize(frame, (int(w * ratio), int(h * ratio))).astype(np.uint8)
        padded_frame[:int(h * ratio), :int(w * ratio)] = resized_frame
        padded_frame = padded_frame.transpose((2, 0, 1))
        padded_frame = np.ascontiguousarray(padded_frame, dtype=np.float32)
        outputs = self.onnx_session.run(None, {self.onnx_session.get_inputs()[0].name: padded_frame[None, :, :, :]})[0]
        grids = []
        expanded_strides = []
        strides = [8, 16, 32]
        if self.with_p6:
            strides.append(64)
        hsizes = [self.input_shape[0] // stride for stride in strides]
        wsizes = [self.input_shape[1] // stride for stride in strides]
        for hsize, wsize, stride in zip(hsizes, wsizes, strides):
            xv, yv = np.meshgrid(np.arange(wsize), np.arange(hsize))
            grid = np.stack((xv, yv), 2).reshape(1, -1, 2)
            grids.append(grid)
            shape = grid.shape[:2]
            expanded_strides.append(np.full((*shape, 1), stride))
        grids = np.concatenate(grids, 1)
        expanded_strides = np.concatenate(expanded_strides, 1)
        outputs[..., :2] = (outputs[..., :2] + grids) * expanded_strides / ratio
        outputs[..., 2:4] = np.exp(outputs[..., 2:4]) * expanded_strides / ratio
        boxes, confs, probs, ids = yolo_utils.get_filtered_result(outputs[0], self.conf_thresh)
        indices = cv2.dnn.NMSBoxes(boxes, confs, self.conf_thresh, self.nms_thresh)
        results = []
        if len(indices) > 0:
            for i in indices.flatten():
                results.append(YoloResult(boxes[i], confs[i], self.classes[ids[i]], probs[i]))
                yolo_utils.draw(frame, boxes[i], self.classes[ids[i]], self.colors[ids[i]], confs[i], self.drawmode)
        return results