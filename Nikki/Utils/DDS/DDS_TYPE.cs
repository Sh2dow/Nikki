﻿using System;



namespace Nikki.Utils.DDS
{
    [Flags]
    public enum DDS_TYPE : uint
    {
        ALPHA = 0x00000002,  // DDPF_ALPHA
        FOURCC = 0x00000004,  // DDPF_FOURCC
        PAL8 = 0x00000020,  // DDPF_PALETTEINDEXED8
        PAL8A = 0x00000021,  // DDPF_PALETTEINDEXED8 | DDPF_ALPHAPIXELS
        RGB = 0x00000040,  // DDPF_RGB
        RGBA = 0x00000041,  // DDPF_RGB | DDPF_ALPHAPIXELS
        LUMINANCE = 0x00020000,  // DDPF_LUMINANCE
        LUMINANCEA = 0x00020001,  // DDPF_LUMINANCE | DDPF_ALPHAPIXELS
        BUMPDUDV = 0x00080000,  // DDPF_BUMPDUDV
    }
}
