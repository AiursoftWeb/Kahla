import { Subject } from 'rxjs';

export class VoiceRecorder {
    private audioChunks: Blob[] = [];
    private recorder?: MediaRecorder;
    private forceStopTimeout?: ReturnType<typeof setTimeout>;
    public gotAudio = new Subject<Blob>();
    public get recording(): boolean {
        return !!(this.recorder && this.recorder.state === 'recording');
    }

    private inited = false;

    constructor(private maxSeconds: number) {}

    public async init() {
        if (this.inited) {
            return;
        }
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        this.recorder = new MediaRecorder(stream);

        this.recorder.addEventListener('dataavailable', event => {
            this.audioChunks.push(event.data);
        });
        this.recorder.addEventListener('stop', () => {
            this.gotAudio.next(new Blob(this.audioChunks));
            this.audioChunks = [];
        });
        this.inited = true;
    }

    public startRecording() {
        if (this.recorder) {
            this.recorder.start();
            this.forceStopTimeout = setTimeout(() => {
                this.stopRecording();
            }, this.maxSeconds * 1000);
        }
    }

    public stopRecording() {
        if (this.recording) {
            this.recorder!.stop();
        }

        if (this.forceStopTimeout) {
            clearTimeout(this.forceStopTimeout);
            this.forceStopTimeout = undefined;
        }
    }
}
