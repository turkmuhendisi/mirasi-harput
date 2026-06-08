package com.mirasiharput.features.locationExperience

import android.content.Context
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableFloatStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.viewinterop.AndroidView
import com.mirasiharput.data.LocationModel
import com.mirasiharput.data.ModelTransform
import io.github.sceneview.SceneView
import io.github.sceneview.math.Position
import io.github.sceneview.math.Rotation
import io.github.sceneview.node.ModelNode

private const val DRAG_ROTATION_SENSITIVITY = 0.4f

@Composable
fun ARModelViewer(
    location: LocationModel,
    modifier: Modifier = Modifier,
    onModelLoadFailed: () -> Unit = {},
) {
    val transform = location.modelTransform
    var userRotationY by remember(location.id) { mutableFloatStateOf(0f) }

    Box(modifier = modifier.fillMaxSize()) {
        AndroidView(
            modifier = Modifier.fillMaxSize(),
            factory = { context ->
                RotatableSceneView(context).apply {
                    loadModelIfNeeded(
                        modelPath = location.modelPath,
                        transform = transform,
                        onFailure = onModelLoadFailed,
                    )
                    applyRotation(transform, userRotationY)
                }
            },
            update = { sceneView ->
                sceneView.loadModelIfNeeded(
                    modelPath = location.modelPath,
                    transform = transform,
                    onFailure = onModelLoadFailed,
                )
                sceneView.applyRotation(transform, userRotationY)
            },
        )

        Box(
            modifier = Modifier
                .fillMaxSize()
                .pointerInput(location.id) {
                    detectDragGestures { change, dragAmount ->
                        change.consume()
                        userRotationY += dragAmount.x * DRAG_ROTATION_SENSITIVITY
                    }
                },
        )
    }
}

private class RotatableSceneView(context: Context) : SceneView(context, isOpaque = false) {

    private var modelNode: ModelNode? = null
    private var loadedModelPath: String? = null

    fun loadModelIfNeeded(
        modelPath: String,
        transform: ModelTransform,
        onFailure: () -> Unit,
    ) {
        if (loadedModelPath == modelPath && modelNode != null) return

        loadedModelPath = modelPath
        clearChildNodes()
        modelNode = null

        runCatching {
            val modelInstance = modelLoader.createModelInstance(assetFileLocation = modelPath)
            ModelNode(
                modelInstance = modelInstance,
                scaleToUnits = transform.scale,
                centerOrigin = Position(transform.positionX, transform.positionY, transform.positionZ),
            ).also { node ->
                modelNode = node
                addChildNode(node)
            }
        }.onFailure {
            loadedModelPath = null
            onFailure()
        }
    }

    fun applyRotation(transform: ModelTransform, userRotationY: Float) {
        val node = modelNode ?: return
        node.transform(
            position = Position(transform.positionX, transform.positionY, transform.positionZ),
            rotation = Rotation(
                x = transform.rotationX,
                y = transform.rotationY + userRotationY,
                z = transform.rotationZ,
            ),
        )
    }
}
