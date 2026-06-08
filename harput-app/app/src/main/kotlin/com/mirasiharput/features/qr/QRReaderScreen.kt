package com.mirasiharput.features.qr

import android.Manifest
import android.content.pm.PackageManager
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.camera.view.PreviewView
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.content.ContextCompat
import com.mirasiharput.ui.theme.BackgroundBlack
import com.mirasiharput.ui.theme.ButtonTextBlack
import com.mirasiharput.ui.theme.ButtonWhite

@Composable
fun QRReaderScreen(
    onQrScanned: (String) -> Unit,
    onCameraPermissionDenied: () -> Unit,
    onBack: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current
    var hasCameraPermission by remember {
        mutableStateOf(
            ContextCompat.checkSelfPermission(context, Manifest.permission.CAMERA) ==
                PackageManager.PERMISSION_GRANTED,
        )
    }

    val permissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission(),
    ) { granted ->
        hasCameraPermission = granted
        if (!granted) {
            onCameraPermissionDenied()
        }
    }

    LaunchedEffect(Unit) {
        if (!hasCameraPermission) {
            permissionLauncher.launch(Manifest.permission.CAMERA)
        }
    }

    val scannerController = remember {
        QRScannerController(
            context = context,
            lifecycleOwner = lifecycleOwner,
            onQrScanned = onQrScanned,
        )
    }

    DisposableEffect(Unit) {
        onDispose { scannerController.release() }
    }

    Box(
        modifier = modifier
            .fillMaxSize()
            .background(BackgroundBlack),
    ) {
        if (hasCameraPermission) {
            AndroidView(
                factory = { ctx ->
                    PreviewView(ctx).also { previewView ->
                        scannerController.bindPreview(previewView)
                    }
                },
                modifier = Modifier.fillMaxSize(),
            )
        }

        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(horizontal = 24.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            Spacer(modifier = Modifier.weight(1f))

            QrScanFrame(
                modifier = Modifier.size(260.dp),
            )

            Spacer(modifier = Modifier.height(16.dp))

            Text(
                text = "Scan QR",
                color = Color.White.copy(alpha = 0.85f),
                fontSize = 14.sp,
            )

            Spacer(modifier = Modifier.weight(1f))

            Button(
                onClick = {
                    if (!hasCameraPermission) {
                        permissionLauncher.launch(Manifest.permission.CAMERA)
                        return@Button
                    }
                    scannerController.startScanning()
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp)
                    .padding(bottom = 16.dp),
                shape = RoundedCornerShape(28.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = ButtonWhite,
                    contentColor = ButtonTextBlack,
                ),
            ) {
                Text(
                    text = "SCAN",
                    fontWeight = FontWeight.Bold,
                    fontSize = 18.sp,
                )
            }
        }

        IconButton(
            onClick = onBack,
            modifier = Modifier
                .align(Alignment.TopStart)
                .padding(16.dp)
                .size(48.dp)
                .background(Color.Black.copy(alpha = 0.45f), RoundedCornerShape(30.dp)),
        ) {
            Icon(
                imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                contentDescription = "Geri",
                tint = Color.White,
            )
        }
    }
}

@Composable
private fun QrScanFrame(modifier: Modifier = Modifier) {
    val cornerColor = Color.White
    val strokeWidth = 4f
    val cornerLength = 48f

    Canvas(modifier = modifier) {
        val width = size.width
        val height = size.height

        fun drawCorner(
            start: Offset,
            horizontalEnd: Offset,
            verticalEnd: Offset,
        ) {
            drawLine(
                color = cornerColor,
                start = start,
                end = horizontalEnd,
                strokeWidth = strokeWidth,
                cap = StrokeCap.Round,
            )
            drawLine(
                color = cornerColor,
                start = start,
                end = verticalEnd,
                strokeWidth = strokeWidth,
                cap = StrokeCap.Round,
            )
        }

        drawCorner(
            start = Offset(0f, 0f),
            horizontalEnd = Offset(cornerLength, 0f),
            verticalEnd = Offset(0f, cornerLength),
        )
        drawCorner(
            start = Offset(width, 0f),
            horizontalEnd = Offset(width - cornerLength, 0f),
            verticalEnd = Offset(width, cornerLength),
        )
        drawCorner(
            start = Offset(0f, height),
            horizontalEnd = Offset(cornerLength, height),
            verticalEnd = Offset(0f, height - cornerLength),
        )
        drawCorner(
            start = Offset(width, height),
            horizontalEnd = Offset(width - cornerLength, height),
            verticalEnd = Offset(width, height - cornerLength),
        )
    }
}
