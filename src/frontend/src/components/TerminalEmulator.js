import React, { useEffect, useRef, useState, useCallback } from 'react';
import { Terminal } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';
import 'xterm/css/xterm.css';
import { emulatorApi } from '../services/api';

export default function TerminalEmulator({ sessionId, onStatusUpdate }) {
  const terminalRef = useRef(null);
  const termRef = useRef(null);
  const fitAddonRef = useRef(null);
  const [outputBuffer, setOutputBuffer] = useState('');
  
  useEffect(() => {
    if (!terminalRef.current || termRef.current) return;
    
    const term = new Terminal({
      theme: {
        background: '#0f0f23',
        foreground: '#00ff00',
        cursor: '#00ff00',
        cursorAccent: '#0f0f23',
        selectionBackground: '#0f3460',
        black: '#000000',
        red: '#e94560',
        green: '#00ff00',
        yellow: '#ffff00',
        blue: '#0f3460',
        magenta: '#e94560',
        cyan: '#00ffff',
        white: '#ffffff',
        brightBlack: '#666666',
        brightRed: '#ff6b8a',
        brightGreen: '#00ff00',
        brightYellow: '#ffff00',
        brightBlue: '#1a4a7a',
        brightMagenta: '#ff6b8a',
        brightCyan: '#00ffff',
        brightWhite: '#ffffff'
      },
      fontFamily: 'Courier New, monospace',
      fontSize: 14,
      cursorBlink: true,
      scrollback: 5000
    });
    
    const fitAddon = new FitAddon();
    term.loadAddon(fitAddon);
    term.open(terminalRef.current);
    
    setTimeout(() => fitAddon.fit(), 100);
    
    termRef.current = term;
    fitAddonRef.current = fitAddon;
    
    term.writeln('\x1b[1;31m========================================\x1b[0m');
    term.writeln('\x1b[1;31m  Intel 8080 Emulator - CP/M 2.2\x1b[0m');
    term.writeln('\x1b[1;31m========================================\x1b[0m');
    term.writeln('');
    term.writeln('\x1b[1;33mType HELP for available commands\x1b[0m');
    term.writeln('');
    term.write('A>');
    
    const handleResize = () => {
      fitAddon.fit();
    };
    
    window.addEventListener('resize', handleResize);
    
    return () => {
      window.removeEventListener('resize', handleResize);
      term.dispose();
      termRef.current = null;
    };
  }, []);
  
  const pollOutput = useCallback(async () => {
    if (!sessionId || !termRef.current) return;
    
    try {
      const status = await emulatorApi.getSessionStatus(sessionId);
      if (status.output && status.output !== outputBuffer) {
        const newOutput = status.output.substring(outputBuffer.length);
        termRef.current.write(newOutput.replace(/\n/g, '\r\n'));
        setOutputBuffer(status.output);
        onStatusUpdate?.(status);
      }
    } catch (e) {
      console.error('Poll error:', e);
    }
  }, [sessionId, outputBuffer, onStatusUpdate]);
  
  useEffect(() => {
    if (!sessionId) return;
    
    const interval = setInterval(pollOutput, 200);
    return () => clearInterval(interval);
  }, [sessionId, pollOutput]);
  
  useEffect(() => {
    if (!termRef.current || !sessionId) return;
    
    let currentLine = '';
    
    const disposable = termRef.current.onData(async (data) => {
      const code = data.charCodeAt(0);
      
      if (code === 13) {
        termRef.current.writeln('');
        
        if (currentLine.trim()) {
          try {
            await emulatorApi.sendCommand(sessionId, currentLine);
          } catch (e) {
            termRef.current.writeln('\x1b[1;31mError: ' + e.message + '\x1b[0m');
          }
        }
        
        termRef.current.write('A>');
        currentLine = '';
      } else if (code === 127 || code === 8) {
        if (currentLine.length > 0) {
          currentLine = currentLine.slice(0, -1);
          termRef.current.write('\b \b');
        }
      } else if (code >= 32) {
        currentLine += data;
        termRef.current.write(data);
      }
    });
    
    return () => disposable.dispose();
  }, [sessionId]);
  
  return <div ref={terminalRef} className="terminal-body" />;
}
