package com.mirasiharput.features.locationExperience

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Context
import android.content.Intent
import android.content.res.AssetFileDescriptor
import android.media.MediaPlayer
import android.os.Build
import android.os.IBinder
import android.support.v4.media.MediaMetadataCompat
import android.support.v4.media.session.MediaSessionCompat
import android.support.v4.media.session.PlaybackStateCompat
import androidx.core.app.NotificationCompat
import androidx.core.app.NotificationManagerCompat
import androidx.media.app.NotificationCompat.MediaStyle
import com.mirasiharput.MainActivity
import com.mirasiharput.R

class AudioPlaybackService : Service() {

    private var mediaPlayer: MediaPlayer? = null
    private var currentTitle: String = ""
    private var currentAudioPath: String? = null

    private lateinit var mediaSession: MediaSessionCompat
    private lateinit var notificationManager: NotificationManagerCompat

    private val sessionCallback = object : MediaSessionCompat.Callback() {
        override fun onPlay() {
            resumePlayback()
        }

        override fun onPause() {
            pausePlayback(notify = true)
        }

        override fun onStop() {
            stopPlayback()
        }
    }

    override fun onCreate() {
        super.onCreate()
        notificationManager = NotificationManagerCompat.from(this)
        createNotificationChannel()

        mediaSession = MediaSessionCompat(this, TAG).apply {
            setCallback(sessionCallback)
            isActive = true
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        when (intent?.action) {
            ACTION_PLAY -> {
                val audioPath = intent.getStringExtra(EXTRA_AUDIO_PATH) ?: return START_NOT_STICKY
                val title = intent.getStringExtra(EXTRA_TITLE).orEmpty()
                startPlayback(audioPath, title)
            }

            ACTION_PAUSE -> pausePlayback(notify = true)
            ACTION_RESUME -> resumePlayback()
            ACTION_STOP -> stopPlayback()
        }
        return START_NOT_STICKY
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onDestroy() {
        releasePlayer()
        mediaSession.run {
            isActive = false
            release()
        }
        super.onDestroy()
    }

    private fun startPlayback(audioPath: String, title: String) {
        if (currentAudioPath == audioPath && mediaPlayer != null) {
            resumePlayback()
            return
        }

        releasePlayer()
        currentAudioPath = audioPath
        currentTitle = title.ifBlank { getString(R.string.notification_default_title) }

        runCatching {
            val descriptor: AssetFileDescriptor = assets.openFd(audioPath)
            mediaPlayer = MediaPlayer().apply {
                setDataSource(
                    descriptor.fileDescriptor,
                    descriptor.startOffset,
                    descriptor.length,
                )
                descriptor.close()
                setOnCompletionListener {
                    AudioPlaybackState.setPlaying(false)
                    updatePlaybackState(PlaybackStateCompat.STATE_STOPPED)
                    stopForeground(STOP_FOREGROUND_REMOVE)
                    notificationManager.cancel(NOTIFICATION_ID)
                    stopSelf()
                }
                prepare()
                start()
            }

            AudioPlaybackState.setPlaying(true)
            updateMetadata()
            updatePlaybackState(PlaybackStateCompat.STATE_PLAYING)
            val notification = buildNotification(isPlaying = true)
            startForeground(NOTIFICATION_ID, notification)
        }.onFailure {
            releasePlayer()
            AudioPlaybackState.setPlaying(false)
            stopSelf()
        }
    }

    private fun resumePlayback() {
        val player = mediaPlayer ?: return
        if (!player.isPlaying) {
            player.start()
        }
        AudioPlaybackState.setPlaying(true)
        updatePlaybackState(PlaybackStateCompat.STATE_PLAYING)
        startForeground(NOTIFICATION_ID, buildNotification(isPlaying = true))
    }

    private fun pausePlayback(notify: Boolean) {
        mediaPlayer?.let { player ->
            if (player.isPlaying) {
                player.pause()
            }
        }
        AudioPlaybackState.setPlaying(false)
        updatePlaybackState(PlaybackStateCompat.STATE_PAUSED)
        if (notify) {
            notificationManager.notify(NOTIFICATION_ID, buildNotification(isPlaying = false))
            stopForeground(STOP_FOREGROUND_DETACH)
        }
    }

    private fun stopPlayback() {
        releasePlayer()
        AudioPlaybackState.setPlaying(false)
        updatePlaybackState(PlaybackStateCompat.STATE_STOPPED)
        stopForeground(STOP_FOREGROUND_REMOVE)
        notificationManager.cancel(NOTIFICATION_ID)
        stopSelf()
    }

    private fun releasePlayer() {
        mediaPlayer?.let { player ->
            player.setOnCompletionListener(null)
            runCatching {
                if (player.isPlaying) {
                    player.stop()
                }
                player.release()
            }
        }
        mediaPlayer = null
        currentAudioPath = null
    }

    private fun updateMetadata() {
        mediaSession.setMetadata(
            MediaMetadataCompat.Builder()
                .putString(MediaMetadataCompat.METADATA_KEY_TITLE, currentTitle)
                .putString(MediaMetadataCompat.METADATA_KEY_ARTIST, getString(R.string.notification_artist))
                .putString(MediaMetadataCompat.METADATA_KEY_ALBUM, getString(R.string.app_name))
                .build(),
        )
    }

    private fun updatePlaybackState(state: Int) {
        mediaSession.setPlaybackState(
            PlaybackStateCompat.Builder()
                .setActions(
                    PlaybackStateCompat.ACTION_PLAY or
                        PlaybackStateCompat.ACTION_PAUSE or
                        PlaybackStateCompat.ACTION_STOP,
                )
                .setState(state, PlaybackStateCompat.PLAYBACK_POSITION_UNKNOWN, 1f)
                .build(),
        )
    }

    private fun buildNotification(isPlaying: Boolean): Notification {
        val contentIntent = PendingIntent.getActivity(
            this,
            0,
            Intent(this, MainActivity::class.java).apply {
                flags = Intent.FLAG_ACTIVITY_SINGLE_TOP
            },
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT,
        )

        val toggleIntent = PendingIntent.getService(
            this,
            1,
            Intent(this, AudioPlaybackService::class.java).apply {
                action = if (isPlaying) ACTION_PAUSE else ACTION_RESUME
            },
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT,
        )

        val stopIntent = PendingIntent.getService(
            this,
            2,
            Intent(this, AudioPlaybackService::class.java).apply {
                action = ACTION_STOP
            },
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT,
        )

        val toggleLabel = if (isPlaying) {
            getString(R.string.notification_action_pause)
        } else {
            getString(R.string.notification_action_play)
        }
        val toggleIcon = if (isPlaying) {
            R.drawable.ic_notification_pause
        } else {
            R.drawable.ic_notification_play
        }

        return NotificationCompat.Builder(this, CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_notification_media)
            .setContentTitle(currentTitle)
            .setContentText(getString(R.string.notification_playing))
            .setContentIntent(contentIntent)
            .setVisibility(NotificationCompat.VISIBILITY_PUBLIC)
            .setOnlyAlertOnce(true)
            .setOngoing(isPlaying)
            .addAction(toggleIcon, toggleLabel, toggleIntent)
            .addAction(
                R.drawable.ic_notification_stop,
                getString(R.string.notification_action_stop),
                stopIntent,
            )
            .setStyle(
                MediaStyle()
                    .setMediaSession(mediaSession.sessionToken)
                    .setShowActionsInCompactView(0, 1),
            )
            .build()
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return

        val channel = NotificationChannel(
            CHANNEL_ID,
            getString(R.string.notification_channel_name),
            NotificationManager.IMPORTANCE_LOW,
        ).apply {
            description = getString(R.string.notification_channel_description)
            setShowBadge(false)
        }

        val manager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
        manager.createNotificationChannel(channel)
    }

    companion object {
        private const val TAG = "MirasiHarputAudio"
        private const val CHANNEL_ID = "mirasi_harput_audio"
        private const val NOTIFICATION_ID = 1001

        const val ACTION_PLAY = "com.mirasiharput.action.PLAY"
        const val ACTION_PAUSE = "com.mirasiharput.action.PAUSE"
        const val ACTION_RESUME = "com.mirasiharput.action.RESUME"
        const val ACTION_STOP = "com.mirasiharput.action.STOP"
        const val EXTRA_AUDIO_PATH = "extra_audio_path"
        const val EXTRA_TITLE = "extra_title"
    }
}
