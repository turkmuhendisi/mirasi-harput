package com.mirasiharput

import androidx.activity.compose.BackHandler
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Snackbar
import androidx.compose.material3.SnackbarHost
import androidx.compose.material3.SnackbarHostState
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mirasiharput.features.certificate.CertificateClaimScreen
import com.mirasiharput.features.home.HomeScreen
import com.mirasiharput.features.locationExperience.LocationExperienceScreen
import com.mirasiharput.features.locationExperience.LocationExperienceViewModel
import com.mirasiharput.features.qr.QRReaderScreen
import com.mirasiharput.navigation.AppExperienceState
import com.mirasiharput.ui.theme.BackgroundBlack
import com.mirasiharput.ui.theme.MirasiHarputTheme
import com.mirasiharput.ui.theme.TextPrimary

@Composable
fun MirasiHarputApp(
    viewModel: LocationExperienceViewModel = viewModel(),
) {
    MirasiHarputTheme {
        val appState by viewModel.appState.collectAsState()
        val toastMessage by viewModel.toastMessage.collectAsState()
        val visitedLocations by viewModel.visitedLocations.collectAsState()
        val certificateEarned by viewModel.certificateEarned.collectAsState()
        val certificateUiState by viewModel.certificateUiState.collectAsState()
        val snackbarHostState = remember { SnackbarHostState() }

        LaunchedEffect(toastMessage) {
            val message = toastMessage ?: return@LaunchedEffect
            snackbarHostState.showSnackbar(message)
            viewModel.dismissToast()
        }

        Scaffold(
            modifier = Modifier.fillMaxSize(),
            containerColor = BackgroundBlack,
            snackbarHost = {
                SnackbarHost(hostState = snackbarHostState) { data ->
                    Snackbar(
                        containerColor = BackgroundBlack,
                        contentColor = TextPrimary,
                    ) {
                        Text(data.visuals.message)
                    }
                }
            },
        ) { padding ->
            val screenModifier = Modifier
                .fillMaxSize()
                .padding(padding)

            when (val state = appState) {
                AppExperienceState.Home -> {
                    HomeScreen(
                        visitedLocationIds = visitedLocations,
                        certificateEarned = certificateEarned,
                        onStartExploring = viewModel::startExploring,
                        onClaimCertificate = viewModel::openCertificateClaim,
                        modifier = screenModifier,
                    )
                }

                AppExperienceState.QrReader -> {
                    BackHandler { viewModel.goHome() }
                    QRReaderScreen(
                        onQrScanned = viewModel::onQrScanned,
                        onCameraPermissionDenied = viewModel::onCameraPermissionDenied,
                        onBack = viewModel::goHome,
                        modifier = screenModifier,
                    )
                }

                is AppExperienceState.LocationExperience -> {
                    BackHandler { viewModel.returnToQrReader() }
                    LocationExperienceScreen(
                        state = state,
                        onBack = viewModel::returnToQrReader,
                        onAudioToggle = viewModel::toggleAudio,
                        onModelLoadFailed = viewModel::onModelLoadFailed,
                        modifier = screenModifier,
                    )
                }

                AppExperienceState.CertificateClaim -> {
                    BackHandler { viewModel.goHome() }
                    CertificateClaimScreen(
                        uiState = certificateUiState,
                        onSubmit = viewModel::submitCertificate,
                        onBack = viewModel::goHome,
                        onDone = viewModel::goHome,
                        modifier = screenModifier,
                    )
                }
            }
        }
    }
}
