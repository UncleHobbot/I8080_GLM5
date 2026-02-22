import React, { useState, useEffect, useCallback } from 'react';
import TerminalEmulator from './components/TerminalEmulator';
import CpuStatus from './components/CpuStatus';
import MemoryViewer from './components/MemoryViewer';
import { emulatorApi } from './services/api';

export default function App() {
  const [sessionId, setSessionId] = useState(null);
  const [cpuState, setCpuState] = useState(null);
  const [activeTab, setActiveTab] = useState('terminal');
  const [loading, setLoading] = useState(false);
  
  const createSession = useCallback(async () => {
    setLoading(true);
    try {
      const { sessionId: id } = await emulatorApi.createSession();
      setSessionId(id);
    } catch (e) {
      console.error('Failed to create session:', e);
    }
    setLoading(false);
  }, []);
  
  useEffect(() => {
    createSession();
  }, [createSession]);
  
  const handleStatusUpdate = (status) => {
    setCpuState(status.cpuState);
  };
  
  const handleReset = async () => {
    if (sessionId) {
      await emulatorApi.resetCpu(sessionId);
      setCpuState(null);
    }
  };
  
  const handleStep = async () => {
    if (sessionId) {
      const state = await emulatorApi.stepProgram(sessionId);
      setCpuState(state);
    }
  };
  
  const handleNewSession = async () => {
    if (sessionId) {
      await emulatorApi.deleteSession(sessionId);
    }
    await createSession();
  };
  
  return (
    <div className="terminal-container">
      <div className="terminal-header">
        <div className="terminal-title">Intel 8080 Emulator</div>
        <div className="status-bar">
          <div className="status-item">
            Session: <span className="status-value">
              {sessionId ? sessionId.substring(0, 8) : '...'}
            </span>
          </div>
          <div className="status-item">
            PC: <span className="status-value">
              {cpuState ? `0x${cpuState.pc.toString(16).toUpperCase()}` : '0x0000'}
            </span>
          </div>
        </div>
      </div>
      
      <div style={{ display: 'flex', gap: '10px', padding: '10px', background: '#16213e' }}>
        <button 
          className={`btn ${activeTab === 'terminal' ? 'primary' : ''}`}
          onClick={() => setActiveTab('terminal')}
        >
          Terminal
        </button>
        <button 
          className={`btn ${activeTab === 'cpu' ? 'primary' : ''}`}
          onClick={() => setActiveTab('cpu')}
        >
          CPU Status
        </button>
        <button 
          className={`btn ${activeTab === 'memory' ? 'primary' : ''}`}
          onClick={() => setActiveTab('memory')}
        >
          Memory
        </button>
      </div>
      
      {loading ? (
        <div style={{ padding: '20px', color: '#888' }}>Initializing emulator...</div>
      ) : (
        <>
          {activeTab === 'terminal' && sessionId && (
            <TerminalEmulator 
              sessionId={sessionId} 
              onStatusUpdate={handleStatusUpdate}
            />
          )}
          
          {activeTab === 'cpu' && (
            <div style={{ padding: '20px' }}>
              <h3 style={{ marginBottom: '10px', color: '#e94560' }}>CPU Registers</h3>
              <CpuStatus cpuState={cpuState} />
            </div>
          )}
          
          {activeTab === 'memory' && sessionId && (
            <div style={{ padding: '20px' }}>
              <h3 style={{ marginBottom: '10px', color: '#e94560' }}>Memory Viewer</h3>
              <MemoryViewer sessionId={sessionId} />
            </div>
          )}
        </>
      )}
      
      <div className="controls">
        <button className="btn" onClick={handleStep} disabled={!sessionId}>
          Step
        </button>
        <button className="btn" onClick={handleReset} disabled={!sessionId}>
          Reset
        </button>
        <button className="btn" onClick={handleNewSession}>
          New Session
        </button>
      </div>
    </div>
  );
}
