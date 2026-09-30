import React, { useEffect, useState, useRef } from 'react';
import {
  Table,
  Card,
  Tag,
  Button,
  Modal,
  Form,
  Select,
  Progress,
  Space,
  Tooltip,
  Input,
  Popconfirm,
  Badge,
  Radio,
  message,
} from 'antd';
import {
  SendOutlined,
  DesktopOutlined,
  ReloadOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import api, { getDeployments, getSoftware, getComputers } from '../services/api';

const statusColor: Record<string, string> = {
  Success: 'green',
  Installing: 'blue',
  Downloading: 'cyan',
  Failed: 'red',
  Pending: 'orange',
};

const statusLabel: Record<string, string> = {
  Success: '✅ Kuruldu',
  Installing: '⚙️ Yükleniyor...',
  Downloading: '⬇️ İndiriliyor...',
  Failed: '❌ Başarısız',
  Pending: '⏳ Bekliyor',
};

export const DeploymentsPage: React.FC = () => {
  const [jobs, setJobs] = useState<any[]>([]);
  const [softwareList, setSoftwareList] = useState<any[]>([]);
  const [computers, setComputers] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [selectedSoftwareVersions, setSelectedSoftwareVersions] = useState<any[]>([]);
  const [form] = Form.useForm();
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const fetchJobs = async (silent = false) => {
    if (!silent) setLoading(true);
    try {
      const res = await getDeployments();
      setJobs(res.data);
    } catch {
      if (!silent) message.error('Görevler yüklenemedi.');
    } finally {
      if (!silent) setLoading(false);
    }
  };

  const fetchDropdownData = async () => {
    try {
      const sRes = await getSoftware();
      const cRes = await getComputers();
      setSoftwareList(sRes.data || []);
      setComputers(cRes.data || []);
    } catch {}
  };

  useEffect(() => {
    fetchJobs();
    fetchDropdownData();
    intervalRef.current = setInterval(() => fetchJobs(true), 6000);
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);

  const handleSoftwareChange = (softwareId: string) => {
    const sw = softwareList.find((s) => s.id === softwareId);
    setSelectedSoftwareVersions(sw?.versions ?? []);
    form.setFieldsValue({ softwareVersionId: undefined });
  };

  const handleCreateJob = async (values: any) => {
    const payload = {
      action: values.action || 1,
      softwareId: values.softwareId,
      softwareVersionId: values.softwareVersionId,
      targetComputerIds: values.targetComputerIds,
      description: values.description || (values.action === 5 ? 'Sadece Dosya/Klasör Aktarma Görevi' : 'Web Panelinden Başlatılan Kurulum Görevi'),
    };
    try {
      await api.post('/Deployments', payload);
      message.success('Dağıtım görevi oluşturuldu! Agent 10 saniye içinde işlemi başlatacak.');
      setIsModalOpen(false);
      form.resetFields();
      setTimeout(() => fetchJobs(), 1000);
    } catch {
      message.error('Dağıtım görevi oluşturulamadı.');
    }
  };

  const handleDeleteJob = async (id: string) => {
    try {
      await api.delete(`/Deployments/${id}`);
      message.success('Görev silindi.');
      fetchJobs();
    } catch {
      message.error('Görev silinemedi.');
    }
  };

  const columns = [
    {
      title: 'Görev No',
      dataIndex: 'jobNumber',
      key: 'jobNumber',
      render: (v: string) => (
        <strong style={{ fontFamily: 'monospace', color: '#1677ff' }}>{v}</strong>
      ),
    },
    {
      title: 'Yazılım & Sürüm',
      key: 'software',
      render: (_: any, record: any) => (
        <Space direction="vertical" size={2}>
          <strong>{record.softwareName || 'Genel Yazılım'}</strong>
          <Tag color="green">v{record.version || '1.0'}</Tag>
        </Space>
      ),
    },
    {
      title: 'Görev Tipi / İşlem',
      dataIndex: 'action',
      key: 'action',
      render: (action: string) => {
        if (action === 'CopyOnly' || action === '5') {
          return <Tag color="purple">📁 Sadece Kopyala / Aktar</Tag>;
        }
        return <Tag color="blue">⚙️ Sessiz Kurulum (Install)</Tag>;
      },
    },
    {
      title: 'Genel İlerleme / Durum',
      key: 'status',
      render: (_: any, record: any) => {
        const percent =
          record.totalTargets > 0
            ? Math.round((record.successCount / record.totalTargets) * 100)
            : 0;
        
        // Eğer hedef bilgisayarların tümü başarısız veya zaman aşımında ise
        const hasFailedTargets = record.failedCount > 0 || record.targetComputers?.some((t: any) => {
          if (t.status === 'Failed') return true;
          if (t.status === 'Pending' || t.status === 'Downloading' || t.status === 'Installing') {
            const dateStr = t.lastSeen ? (t.lastSeen.endsWith('Z') || t.lastSeen.includes('+') ? t.lastSeen : t.lastSeen + 'Z') : null;
            const diff = dateStr ? (new Date().getTime() - new Date(dateStr).getTime()) / 1000 : 999;
            return diff > 45; // 45 saniyedir haber alınamıyorsa zaman aşımı / yanıt vermiyor
          }
          return false;
        });

        const isCompleted = record.status === 'Completed' || record.successCount === record.totalTargets;

        if (isCompleted) {
          return (
            <Space direction="vertical" style={{ width: 140 }}>
              <Tag color="green">✅ Tamamlandı</Tag>
              <Progress percent={100} size="small" status="success" />
            </Space>
          );
        }

        if (hasFailedTargets && record.successCount === 0) {
          return (
            <Space direction="vertical" style={{ width: 140 }}>
              <Tag color="red">❌ Yanıt Alınamadı / Hata</Tag>
              <Progress percent={percent} size="small" status="exception" />
            </Space>
          );
        }

        return (
          <Space direction="vertical" style={{ width: 140 }}>
            <Tag color="processing">🔄 Devam Ediyor</Tag>
            <Progress percent={percent} size="small" />
          </Space>
        );
      },
    },
    {
      title: 'Hedef Özeti',
      key: 'counts',
      render: (_: any, r: any) => (
        <Space>
          <Tag color="blue">{r.totalTargets} PC</Tag>
          <Tag color="green">{r.successCount} ✅</Tag>
          <Tag color="red">{r.failedCount} ❌</Tag>
        </Space>
      ),
    },
    {
      title: 'Oluşturulma',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (v: string) =>
        v ? (
          <Tooltip title={new Date(v).toLocaleString('tr-TR')}>
            <Space>
              <ClockCircleOutlined />
              {new Date(v).toLocaleDateString('tr-TR')}
            </Space>
          </Tooltip>
        ) : (
          '—'
        ),
    },
    {
      title: 'İşlemler',
      key: 'actions',
      render: (_: any, record: any) => (
        <Popconfirm
          title="Bu görevi silmek istediğinize emin misiniz?"
          onConfirm={() => handleDeleteJob(record.id)}
          okText="Evet, Sil"
          cancelText="İptal"
        >
          <Button danger size="small" icon={<DeleteOutlined />} />
        </Popconfirm>
      ),
    },
  ];

  const expandedRowRender = (record: any) => (
    <Card
      size="small"
      title={
        <Space>
          <DesktopOutlined /> Hedef Bilgisayarlar ve Anlık Canlı Kurulum Durumları
        </Space>
      }
      style={{ margin: 0, background: '#fafafa' }}
    >
      {record.targetComputers && record.targetComputers.length > 0 ? (
        <Table
          size="small"
          pagination={false}
          dataSource={record.targetComputers}
          rowKey="computerId"
          columns={[
            {
              title: 'Bilgisayar Adı',
              dataIndex: 'hostname',
              render: (v: string) => (
                <Space>
                  <DesktopOutlined style={{ color: '#1677ff' }} />
                  <strong>{v}</strong>
                </Space>
              ),
            },
            {
              title: 'IP Adresi',
              dataIndex: 'ipAddress',
              render: (v: string) => (v ? <code>{v}</code> : <Tag>IP Yok</Tag>),
            },
            {
              title: 'Canlı Durum',
              dataIndex: 'status',
              render: (v: string, target: any) => {
                const dateStr = target.lastSeen ? (target.lastSeen.endsWith('Z') || target.lastSeen.includes('+') ? target.lastSeen : target.lastSeen + 'Z') : null;
                const diff = dateStr ? (new Date().getTime() - new Date(dateStr).getTime()) / 1000 : 999;
                const isTimedOut = (target.status === 'Pending' || target.status === 'Downloading' || target.status === 'Installing') && diff > 45;

                if (isTimedOut) {
                  return <Tag color="red">❌ Çevrimdışı / Yanıt Vermiyor</Tag>;
                }

                return (
                  <Tag color={statusColor[v] ?? 'default'}>
                    {statusLabel[v] ?? v}
                  </Tag>
                );
              },
            },
            {
              title: 'Son Mesaj / Rapor',
              dataIndex: 'message',
              render: (v: string, target: any) => {
                const dateStr = target.lastSeen ? (target.lastSeen.endsWith('Z') || target.lastSeen.includes('+') ? target.lastSeen : target.lastSeen + 'Z') : null;
                const diff = dateStr ? (new Date().getTime() - new Date(dateStr).getTime()) / 1000 : 999;
                const isTimedOut = (target.status === 'Pending' || target.status === 'Downloading' || target.status === 'Installing') && diff > 45;

                if (isTimedOut) {
                  return <span style={{ color: '#ff4d4f' }}>⚠️ Agent kapalı veya sunucuyla bağlantısı kesildi. (Son Görülme: {Math.round(diff)} sn önce)</span>;
                }

                return v || 'İşlem bekleniyor...';
              },
            },
          ]}
        />
      ) : (
        <div style={{ color: '#999' }}>Hedef bilgisayar bilgisi bulunamadı.</div>
      )}
    </Card>
  );

  return (
    <Card
      title={
        <Space>
          <span>Yazılım Dağıtım Görevleri (Jobs)</span>
          <Badge count={jobs.length} color="blue" />
        </Space>
      }
      extra={
        <Space>
          <Tooltip title="Yenile">
            <Button icon={<ReloadOutlined />} onClick={() => fetchJobs()} />
          </Tooltip>
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={() => {
              fetchDropdownData();
              setIsModalOpen(true);
            }}
          >
            Yeni Dağıtım Görevi
          </Button>
        </Space>
      }
    >
      <Table
        dataSource={jobs}
        columns={columns}
        rowKey="id"
        loading={loading}
        expandable={{ expandedRowRender }}
        locale={{ emptyText: 'Henüz bir dağıtım görevi oluşturulmadı.' }}
      />

      <Modal
        title="Yeni Yazılım Dağıtımı Başlat"
        open={isModalOpen}
        onCancel={() => {
          setIsModalOpen(false);
          form.resetFields();
        }}
        onOk={() => form.submit()}
        okText="Görevi Başlat"
        cancelText="İptal"
        width={540}
      >
        <Form form={form} layout="vertical" onFinish={handleCreateJob} initialValues={{ action: 1 }}>
          <Form.Item
            name="action"
            label="Dağıtım Eylemi / Tipi"
            rules={[{ required: true, message: 'Lütfen işlem tipini seçin!' }]}
          >
            <Radio.Group optionType="button" buttonStyle="solid">
              <Radio.Button value={1}>⚙️ Sessiz Kurulum Yap (Install)</Radio.Button>
              <Radio.Button value={5}>📁 Sadece Kopyala / Aktar (Copy Only)</Radio.Button>
            </Radio.Group>
          </Form.Item>

          <Form.Item
            name="softwareId"
            label="Dağıtılacak Yazılımı Seçin"
            rules={[{ required: true, message: 'Yazılım seçiniz!' }]}
          >
            <Select
              placeholder="Yazılım Seçiniz"
              onChange={handleSoftwareChange}
              showSearch
              optionFilterProp="children"
            >
              {softwareList.map((s) => (
                <Select.Option key={s.id} value={s.id}>
                  {s.name} {s.publisher ? `(${s.publisher})` : ''}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="softwareVersionId"
            label="Sürüm Seçin"
            rules={[{ required: true, message: 'Sürüm seçiniz!' }]}
          >
            <Select
              placeholder={
                selectedSoftwareVersions.length
                  ? 'Sürüm Seçiniz'
                  : 'Önce Yukarıdan Yazılım Seçin'
              }
              disabled={!selectedSoftwareVersions.length}
            >
              {selectedSoftwareVersions.map((v) => (
                <Select.Option key={v.id} value={v.id}>
                  v{v.version} {v.isCurrent ? '(Aktif Sürüm)' : ''}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="targetComputerIds"
            label="Hedef Bilgisayar(lar) Seçin"
            rules={[{ required: true, message: 'En az 1 bilgisayar seçiniz!' }]}
          >
            <Select
              mode="multiple"
              placeholder="Hedef Bilgisayarları Seçin"
              showSearch
              optionFilterProp="children"
            >
              {computers.map((c) => (
                <Select.Option key={c.id} value={c.id}>
                  🖥️ {c.hostname} {c.ipAddress ? `(${c.ipAddress})` : ''} {c.status === 'Online' ? '🟢' : '🔴'}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item name="description" label="Açıklama / Not (isteğe bağlı)">
            <Input placeholder="Örn: Laboratuvar PC'lerine Güncelleme" />
          </Form.Item>
        </Form>
      </Modal>
    </Card>
  );
};
