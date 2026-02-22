import React, { useState } from 'react';

export default function MemoryViewer({ sessionId }) {
  const [address, setAddress] = useState(0);
  const [memory, setMemory] = useState(null);
  
  const handleView = async () => {
    try {
      const response = await fetch(
        `http://localhost:5000/api/emulator/session/${sessionId}/memory?address=${address}&length=256`
      );
      const data = await response.json();
      setMemory(data);
    } catch (e) {
      console.error(e);
    }
  };
  
  const formatBytes = (bytes, startAddr) => {
    const lines = [];
    for (let i = 0; i < bytes.length; i += 16) {
      const addr = startAddr + i;
      const slice = bytes.slice(i, i + 16);
      
      const hexPart = Array.from(slice)
        .map(b => b.toString(16).toUpperCase().padStart(2, '0'))
        .join(' ');
      
      const asciiPart = Array.from(slice)
        .map(b => (b >= 32 && b < 127) ? String.fromCharCode(b) : '.')
        .join('');
      
      lines.push(
        <div key={addr} className="memory-line">
          <span className="memory-address">
            0x{addr.toString(16).toUpperCase().padStart(4, '0')}
          </span>
          <span className="memory-bytes">{hexPart.padEnd(47, ' ')}</span>
          <span className="memory-ascii">{asciiPart}</span>
        </div>
      );
    }
    return lines;
  };
  
  return (
    <div>
      <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
        <input
          type="number"
          value={address}
          onChange={e => setAddress(parseInt(e.target.value) || 0)}
          placeholder="Address (hex)"
          style={{
            background: '#0f0f23',
            border: '1px solid #0f3460',
            color: '#00ff00',
            padding: '5px 10px',
            fontFamily: 'inherit',
            width: '150px'
          }}
        />
        <button className="btn" onClick={handleView}>View Memory</button>
      </div>
      {memory && (
        <div className="memory-viewer">
          {formatBytes(memory.data, memory.address)}
        </div>
      )}
    </div>
  );
}
