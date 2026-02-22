import React from 'react';

export default function CpuStatus({ cpuState }) {
  if (!cpuState) return null;
  
  const registers = [
    { name: 'A', value: cpuState.a },
    { name: 'B', value: cpuState.b },
    { name: 'C', value: cpuState.c },
    { name: 'D', value: cpuState.d },
    { name: 'E', value: cpuState.e },
    { name: 'H', value: cpuState.h },
    { name: 'L', value: cpuState.l },
    { name: 'SP', value: cpuState.sp },
    { name: 'PC', value: cpuState.pc }
  ];
  
  const flags = [
    { name: 'S', bit: 7 },
    { name: 'Z', bit: 6 },
    { name: 'AC', bit: 4 },
    { name: 'P', bit: 2 },
    { name: 'C', bit: 0 }
  ];
  
  return (
    <div className="cpu-status">
      {registers.map(reg => (
        <div key={reg.name} className="register">
          <div className="register-name">{reg.name}</div>
          <div className="register-value">
            {typeof reg.value === 'number' 
              ? (reg.name === 'SP' || reg.name === 'PC' 
                  ? `0x${reg.value.toString(16).toUpperCase().padStart(4, '0')}`
                  : `0x${reg.value.toString(16).toUpperCase().padStart(2, '0')}`)
              : '0x00'}
          </div>
        </div>
      ))}
      <div className="register">
        <div className="register-name">FLAGS</div>
        <div className="register-value">
          {flags.map(f => (
            <span 
              key={f.name}
              style={{ 
                color: ((cpuState.flags >> f.bit) & 1) ? '#00ff00' : '#666',
                marginRight: '2px'
              }}
            >
              {f.name}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}
