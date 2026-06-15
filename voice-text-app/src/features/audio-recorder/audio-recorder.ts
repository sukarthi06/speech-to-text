import { Component, OnInit, signal } from '@angular/core';
import { AudioVisualizer } from '../audio-visualizer/audio-visualizer';
import { environment } from '../../environments/environment';
import { AudioTranscript } from '../audio-transcript/audio-transcript';
import { TranscriptResponse, TranscriptSegment } from '../../types/transcript-segment';

@Component({
  selector: 'app-audio-recorder',
  imports: [AudioVisualizer, AudioTranscript],
  templateUrl: './audio-recorder.html',
  styleUrl: './audio-recorder.css',
})
export class AudioRecorder implements OnInit {

  private audioContext: AudioContext | null = null;  
  private source: MediaStreamAudioSourceNode | null = null;
  private node: AudioWorkletNode | null = null;
  private stream: MediaStream | null = null;
  private chunkWorker: Worker;

  // Array to store all audio chunks
  allChunks: Float32Array[] = [];
  private chunkCounter = 0;

  public isProcessingAudio = signal(false);

  public transcript = signal('');
  public analyser = signal<AnalyserNode | null>(null);
  public segments = signal<TranscriptSegment[]>([]);

  constructor() {
    // Initialize worker
    this.chunkWorker = new Worker(
      new URL('../../workers/chunk-processor.worker', import.meta.url)
    );

    // Listen to messages from worker
    this.chunkWorker.onmessage = (event: MessageEvent) => {
      //console.log('Processed chunk from worker:', event.data.message);
      //console.log('Received in component:', event.data);
      
      const data: TranscriptResponse = JSON.parse(event.data);
      this.transcript.set(this.transcript() + data.Text);
      this.segments.update(current => [
        ...current,
        ...data.Segments.map(s => ({
          speaker: s.Speaker,
          text: s.Text
        }))
      ]);

      this.isProcessingAudio.set(false);
    };
  }

  ngOnInit() {    
    this.chunkWorker.postMessage({ type: 'init', websocketUrl: environment.websocketUrl });
  }

  processAudioChunk(chunk: Float32Array) {    
    this.allChunks.push(chunk);
    this.chunkWorker.postMessage({ type:'send', chunkNumber: this.chunkCounter++, chunk: chunk });
  }

  async startAudio() {

    this.audioContext = new AudioContext();

    // Load your processor only once per context
    await this.audioContext.audioWorklet.addModule('/assets/audio-processor.js');

    // Log the sample rate to verify it's correct -- Don't delete
    //console.log('Audio context sample rate:', this.audioContext.sampleRate);

    // Create analyser node for visualizer
    const analyserNode = this.audioContext.createAnalyser();
    analyserNode.fftSize = 2048;
    analyserNode.smoothingTimeConstant = 0.8;    
    this.analyser.set(analyserNode);

    // Get microphone
    this.stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    this.source = this.audioContext.createMediaStreamSource(this.stream);
    
    // Create worklet node
    this.node = new AudioWorkletNode(this.audioContext, 'audio-processor');

    // Listen to messages
    this.node.port.onmessage = (event) => {      
      //console.log('Audio chunk:', event.data);
      
      const audioChunk: Float32Array = event.data.data;
      //this.allChunks.push(audioChunk);
      this.processAudioChunk(audioChunk);
    };

    // Connect source -> worklet
    this.source.connect(this.node);
    this.source.connect(analyserNode); // Connect to analyser for visualizer
  }

  stopAudio() {

    console.log('Audio stopped. Waiting 3 seconds for workers to finish processing...');

    this.isProcessingAudio.set(true);
    this.chunkWorker.postMessage({ type: 'stop' });
    // Stop audio immediately
    if (this.source) this.source.disconnect();
    if (this.node) this.node.disconnect();
    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
    }
    if (this.audioContext) this.audioContext.close();

    // Allow workers 3 seconds to process the chunks before resetting everything
    setTimeout(() => {

      // Reset
      this.audioContext = null;
      this.source = null;
      this.node = null;
      this.stream = null;
      this.analyser.set(null);
      
      // Terminate worker after processing
      //this.chunkWorker.postMessage({ type: 'close' });
      //this.chunkWorker.terminate();
      
      this.chunkCounter = 0;
      console.log('Audio stopped');
      
      console.log('Total chunks:', this.allChunks.length);
      this.allChunks = [];
    }, 3000);
  }
}