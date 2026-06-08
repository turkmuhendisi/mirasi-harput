package com.mirasiharput.data

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringSetPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

private val Context.visitDataStore by preferencesDataStore(name = "visit_progress")

/**
 * Ziyaret edilen mekanları cihazda kalıcı olarak saklar.
 * Sertifika kazanımı için her iki mekanın da ziyaret edilmiş olması gerekir.
 */
class VisitProgressRepository(context: Context) {

    private val appContext = context.applicationContext
    private val visitedKey = stringSetPreferencesKey("visited_location_ids")

    val visitedLocationIds: Flow<Set<String>> = appContext.visitDataStore.data
        .map { prefs -> prefs[visitedKey] ?: emptySet() }

    suspend fun markVisited(locationId: String) {
        appContext.visitDataStore.edit { prefs ->
            val current = prefs[visitedKey] ?: emptySet()
            prefs[visitedKey] = current + locationId
        }
    }

    companion object {
        val REQUIRED_LOCATION_IDS = setOf("harput_kalesi", "urartu_sarnici_zindani")

        fun hasEarnedCertificate(visited: Set<String>): Boolean =
            visited.containsAll(REQUIRED_LOCATION_IDS)
    }
}
