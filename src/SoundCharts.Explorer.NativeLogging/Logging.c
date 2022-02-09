//
//  Logging.c
//  SoundCharts.Explorer.NativeLogging
//
//  Created by Phil Hoff on 08.02.22.
//

#include <os/log.h>

extern void Log(os_log_t log, char *message) {
    os_log(log, "%{public}s", message);
}
