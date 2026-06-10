package com.mirasiharput.features.locationExperience

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.mirasiharput.data.LocationModel
import com.mirasiharput.data.LocationRepository
import com.mirasiharput.data.VisitProgressRepository
import com.mirasiharput.data.achievements.AchievementRepository
import com.mirasiharput.data.achievements.QuizAward
import com.mirasiharput.features.certificate.CertificateRepository
import com.mirasiharput.features.certificate.CertificateUiState
import com.mirasiharput.features.qr.QRPayloadParser
import com.mirasiharput.navigation.AppExperienceState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.launchIn
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.onEach
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

class LocationExperienceViewModel(application: Application) : AndroidViewModel(application) {

    private val audioController = AudioController(application)
    private val visitProgressRepository = VisitProgressRepository(application)
    private val certificateRepository = CertificateRepository()
    private val achievementRepository = AchievementRepository(application)

    private val _appState = MutableStateFlow<AppExperienceState>(AppExperienceState.Home)
    val appState: StateFlow<AppExperienceState> = _appState.asStateFlow()

    private val _toastMessage = MutableStateFlow<String?>(null)
    val toastMessage: StateFlow<String?> = _toastMessage.asStateFlow()

    private val _certificateUiState = MutableStateFlow<CertificateUiState>(CertificateUiState.Idle)
    val certificateUiState: StateFlow<CertificateUiState> = _certificateUiState.asStateFlow()

    val visitedLocations: StateFlow<Set<String>> = visitProgressRepository.visitedLocationIds
        .stateIn(viewModelScope, SharingStarted.Eagerly, emptySet())

    val certificateEarned: StateFlow<Boolean> = visitProgressRepository.visitedLocationIds
        .map { VisitProgressRepository.hasEarnedCertificate(it) }
        .stateIn(viewModelScope, SharingStarted.Eagerly, false)

    val totalPoints: StateFlow<Int> = achievementRepository.totalPoints
        .stateIn(viewModelScope, SharingStarted.Eagerly, 0)

    val earnedBadgeIds: StateFlow<Set<String>> = achievementRepository.earnedBadgeIds
        .stateIn(viewModelScope, SharingStarted.Eagerly, emptySet())

    private val _quizAward = MutableStateFlow<QuizAward?>(null)
    val quizAward: StateFlow<QuizAward?> = _quizAward.asStateFlow()

    init {
        audioController.isPlaying
            .onEach { isPlaying ->
                val current = _appState.value
                if (current is AppExperienceState.LocationExperience) {
                    _appState.value = current.copy(isAudioActive = isPlaying)
                }
            }
            .launchIn(viewModelScope)
    }

    // --- Navigation ---

    fun startExploring() {
        _appState.value = AppExperienceState.QrReader
    }

    fun goHome() {
        audioController.release()
        _appState.value = AppExperienceState.Home
    }

    fun openCertificateClaim() {
        _certificateUiState.value = CertificateUiState.Idle
        _appState.value = AppExperienceState.CertificateClaim
    }

    fun onQrScanned(rawPayload: String) {
        val locationId = QRPayloadParser.parse(rawPayload)
        if (locationId == null) {
            showToast(QRPayloadParser.INVALID_QR_MESSAGE)
            return
        }

        val location = LocationRepository.findById(locationId)
        if (location == null) {
            showToast(QRPayloadParser.LOCATION_NOT_FOUND_MESSAGE)
            return
        }

        openLocation(location)
    }

    fun onCameraPermissionDenied() {
        showToast("Kamera izni olmadan QR okuma kullanılamaz.")
    }

    fun returnToQrReader() {
        audioController.release()
        _appState.value = AppExperienceState.QrReader
    }

    // --- AR Deneyimi ---

    fun openArExperience() {
        val current = _appState.value as? AppExperienceState.LocationExperience ?: return
        audioController.release()
        _quizAward.value = null
        _appState.value = AppExperienceState.ArExperience(location = current.location)
    }

    fun exitArExperience() {
        val current = _appState.value as? AppExperienceState.ArExperience ?: return
        _quizAward.value = null
        _appState.value = AppExperienceState.LocationExperience(
            location = current.location,
            isAudioActive = false,
        )
    }

    fun onArSessionFailed() {
        showToast("AR oturumu başlatılamadı. Cihazınız ARCore desteklemiyor olabilir.")
        exitArExperience()
    }

    fun completeQuiz(locationId: String, correctCount: Int) {
        viewModelScope.launch {
            _quizAward.value = achievementRepository.recordQuizResult(locationId, correctCount)
        }
    }

    fun clearQuizAward() {
        _quizAward.value = null
    }

    // --- Audio ---

    fun toggleAudio() {
        val current = _appState.value as? AppExperienceState.LocationExperience ?: return

        if (current.isAudioActive) {
            audioController.pause()
            return
        }

        val prepared = audioController.prepare(
            audioPath = current.location.audioPath,
            title = current.location.title,
        )
        if (!prepared) {
            showToast("Seslendirme şu anda oynatılamıyor.")
            return
        }

        val started = audioController.play()
        if (!started) {
            showToast("Seslendirme şu anda oynatılamıyor.")
        }
    }

    fun onModelLoadFailed() {
        showToast("3D model şu anda yüklenemedi.")
    }

    // --- Certificate ---

    fun submitCertificate(fullName: String, email: String) {
        if (fullName.trim().length < 3) {
            _certificateUiState.value = CertificateUiState.Error("Lütfen geçerli bir ad soyad girin.")
            return
        }
        if (!android.util.Patterns.EMAIL_ADDRESS.matcher(email.trim()).matches()) {
            _certificateUiState.value = CertificateUiState.Error("Lütfen geçerli bir e-posta adresi girin.")
            return
        }

        _certificateUiState.value = CertificateUiState.Submitting
        viewModelScope.launch {
            val result = certificateRepository.requestCertificate(fullName, email)
            _certificateUiState.value = result.fold(
                onSuccess = { CertificateUiState.Success(it) },
                onFailure = { CertificateUiState.Error(it.message ?: "Sertifika gönderilemedi.") },
            )
        }
    }

    fun resetCertificateState() {
        _certificateUiState.value = CertificateUiState.Idle
    }

    fun dismissToast() {
        _toastMessage.value = null
    }

    private fun openLocation(location: LocationModel) {
        audioController.release()
        viewModelScope.launch {
            visitProgressRepository.markVisited(location.id)
        }
        _appState.value = AppExperienceState.LocationExperience(
            location = location,
            isAudioActive = false,
        )
    }

    private fun showToast(message: String) {
        _toastMessage.update { message }
    }

    override fun onCleared() {
        audioController.release()
        super.onCleared()
    }
}
