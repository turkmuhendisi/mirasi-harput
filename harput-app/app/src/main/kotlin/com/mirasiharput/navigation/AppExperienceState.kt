package com.mirasiharput.navigation

import com.mirasiharput.data.LocationModel

sealed interface AppExperienceState {
    data object Home : AppExperienceState

    data object QrReader : AppExperienceState

    data class LocationExperience(
        val location: LocationModel,
        val isAudioActive: Boolean = false,
    ) : AppExperienceState

    data class ArExperience(
        val location: LocationModel,
    ) : AppExperienceState

    data object CertificateClaim : AppExperienceState
}
