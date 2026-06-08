package com.mirasiharput.features.locationExperience

import androidx.camera.view.PreviewView
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import com.mirasiharput.features.camera.CameraPreviewController
import com.mirasiharput.navigation.AppExperienceState
import com.mirasiharput.ui.theme.BackButtonBlack
import com.mirasiharput.ui.theme.BackIconWhite
import com.mirasiharput.ui.theme.BackgroundBlack

@Composable
fun LocationExperienceScreen(
    state: AppExperienceState.LocationExperience,
    onBack: () -> Unit,
    onAudioToggle: () -> Unit,
    onModelLoadFailed: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current

    val cameraController = remember {
        CameraPreviewController(
            context = context,
            lifecycleOwner = lifecycleOwner,
        )
    }

    DisposableEffect(Unit) {
        onDispose { cameraController.release() }
    }

    Column(
        modifier = modifier
            .fillMaxSize()
            .background(BackgroundBlack),
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .weight(0.55f),
        ) {
            AndroidView(
                factory = { ctx ->
                    PreviewView(ctx).also { previewView ->
                        cameraController.bindPreview(previewView)
                    }
                },
                modifier = Modifier.fillMaxSize(),
            )

            ARModelViewer(
                location = state.location,
                onModelLoadFailed = onModelLoadFailed,
            )

            IconButton(
                onClick = onBack,
                modifier = Modifier
                    .align(Alignment.TopStart)
                    .padding(16.dp)
                    .size(48.dp)
                    .background(BackButtonBlack, RoundedCornerShape(30.dp)),
            ) {
                Icon(
                    imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = "Geri",
                    tint = BackIconWhite,
                )
            }
        }

        LocationInfoPanel(
            description = state.location.description,
            isAudioActive = state.isAudioActive,
            onAudioToggle = onAudioToggle,
            modifier = Modifier
                .fillMaxWidth()
                .weight(0.45f),
        )
    }
}
