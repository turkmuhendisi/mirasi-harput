package com.mirasiharput.features.locationExperience

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Pause
import androidx.compose.material.icons.automirrored.filled.VolumeUp
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.mirasiharput.ui.theme.BackgroundBlack
import com.mirasiharput.ui.theme.ButtonWhite
import com.mirasiharput.ui.theme.TextPrimary

@Composable
fun LocationInfoPanel(
    description: String,
    isAudioActive: Boolean,
    onAudioToggle: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val scrollState = rememberScrollState()
    val panelShape = RoundedCornerShape(topStart = 28.dp, topEnd = 28.dp)

    Box(
        modifier = modifier
            .fillMaxWidth()
            .background(BackgroundBlack, panelShape),
    ) {
        IconButton(
            onClick = onAudioToggle,
            modifier = Modifier
                .align(Alignment.TopEnd)
                .padding(top = 8.dp, end = 8.dp)
                .background(ButtonWhite, RoundedCornerShape(30.dp)),
        ) {
            Icon(
                imageVector = if (isAudioActive) Icons.Default.Pause else Icons.AutoMirrored.Filled.VolumeUp,
                contentDescription = if (isAudioActive) "Sesi durdur" else "Seslendirmeyi başlat",
                tint = BackgroundBlack,
            )
        }

        Text(
            text = description,
            color = TextPrimary,
            fontSize = 16.sp,
            fontWeight = FontWeight.SemiBold,
            lineHeight = 24.sp,
            modifier = Modifier
                .fillMaxWidth()
                .padding(start = 20.dp, end = 20.dp, top = 56.dp, bottom = 24.dp)
                .verticalScroll(scrollState),
        )
    }
}
