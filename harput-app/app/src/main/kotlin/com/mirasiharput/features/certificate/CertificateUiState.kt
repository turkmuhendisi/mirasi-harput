package com.mirasiharput.features.certificate

sealed interface CertificateUiState {
    data object Idle : CertificateUiState
    data object Submitting : CertificateUiState
    data class Success(val message: String) : CertificateUiState
    data class Error(val message: String) : CertificateUiState
}
