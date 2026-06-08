package com.mirasiharput.features.locationExperience

import android.content.Context
import android.content.Intent
import androidx.core.content.ContextCompat
import kotlinx.coroutines.flow.StateFlow

class AudioController(context: Context) {

    private val appContext = context.applicationContext

    val isPlaying: StateFlow<Boolean> = AudioPlaybackState.isPlaying

    fun prepare(audioPath: String, title: String): Boolean {
        return try {
            appContext.assets.openFd(audioPath).close()
            startService(
                action = AudioPlaybackService.ACTION_PLAY,
                audioPath = audioPath,
                title = title,
            )
            true
        } catch (_: Exception) {
            false
        }
    }

    fun play(): Boolean {
        startService(action = AudioPlaybackService.ACTION_RESUME)
        return true
    }

    fun pause() {
        startService(action = AudioPlaybackService.ACTION_PAUSE)
    }

    fun stop() {
        startService(action = AudioPlaybackService.ACTION_STOP)
    }

    fun toggle(): Boolean {
        return if (isPlaying.value) {
            pause()
            true
        } else {
            play()
        }
    }

    fun release() {
        stop()
    }

    private fun startService(
        action: String,
        audioPath: String? = null,
        title: String? = null,
    ) {
        val intent = Intent(appContext, AudioPlaybackService::class.java).apply {
            this.action = action
            audioPath?.let { putExtra(AudioPlaybackService.EXTRA_AUDIO_PATH, it) }
            title?.let { putExtra(AudioPlaybackService.EXTRA_TITLE, it) }
        }

        if (action == AudioPlaybackService.ACTION_PLAY) {
            ContextCompat.startForegroundService(appContext, intent)
        } else {
            appContext.startService(intent)
        }
    }
}
