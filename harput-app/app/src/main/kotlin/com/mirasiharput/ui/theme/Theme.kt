package com.mirasiharput.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable

private val DarkColorScheme = darkColorScheme(
    background = BackgroundBlack,
    surface = PanelBlack,
    onBackground = TextPrimary,
    onSurface = TextPrimary,
    primary = ButtonWhite,
    onPrimary = ButtonTextBlack,
)

@Composable
fun MirasiHarputTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = DarkColorScheme,
        typography = Typography,
        content = content,
    )
}
